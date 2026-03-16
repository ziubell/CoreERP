using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly SignInManager<ApplicationIdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationIdentityUser> userManager,
        SignInManager<ApplicationIdentityUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Diagnostica: verifica che API e DB siano raggiungibili
    /// </summary>
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            // Test DB connection
            var user = await _userManager.FindByEmailAsync("test@test.com");
            return Ok(new { status = "ok", db = "connected", userFound = user is not null, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return Ok(new { status = "error", db = "failed", error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Tentativo login per: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogWarning("Utente non trovato: {Email}", request.Email);
                return Unauthorized(new { errors = new { email = "Credenziali non valide" } });
            }

            _logger.LogInformation("Utente trovato: {Email}, verifico password...", request.Email);

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account bloccato per troppi tentativi: {Email}", request.Email);
                return Unauthorized(new { errors = new { email = "Account temporaneamente bloccato. Riprova tra qualche minuto." } });
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password errata per: {Email}", request.Email);
                return Unauthorized(new { errors = new { email = "Credenziali non valide" } });
            }

            _logger.LogInformation("Password corretta, genero token per: {Email}", request.Email);

            // Update login timestamps
            user.DataUltimoLogin = user.DataLogin;
            user.DataLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var tokenResponse = await GenerateTokenResponse(user);

            _logger.LogInformation("Login effettuato con successo: {Email}", request.Email);

            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERRORE LOGIN per {Email}: {Message}", request.Email, ex.ToString());
            return StatusCode(500, new { message = $"Errore interno: {ex.Message}", detail = ex.ToString() });
        }
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always return OK to prevent email enumeration
        if (user is null)
        {
            _logger.LogWarning("Richiesta reset password per email non esistente: {Email}", request.Email);
            return Ok(new { message = "Se l'email è associata a un account, riceverai le istruzioni." });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: Send email with reset link
        // The link should point to: {frontendUrl}/reset-password?token={token}&email={email}
        _logger.LogInformation("Token reset password generato per: {Email}. Token: {Token}", request.Email, token);

        return Ok(new { message = "Se l'email è associata a un account, riceverai le istruzioni." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return BadRequest(new { message = "Richiesta non valida." });
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Reset password fallito per {Email}: {Errors}", request.Email, errors);
            return BadRequest(new { message = "Il link di reset potrebbe essere scaduto. Richiedi un nuovo link." });
        }

        _logger.LogInformation("Password reimpostata per: {Email}", request.Email);

        return Ok(new { message = "Password reimpostata con successo." });
    }

    [HttpGet("microsoft-login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult MicrosoftLogin([FromQuery] string? returnUrl = "/")
    {
        var clientId = _configuration["AzureAd:ClientId"];

        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest(new { message = "Autenticazione Microsoft non configurata." });
        }

        var tenantId = _configuration["AzureAd:TenantId"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/microsoft-callback";
        var state = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnUrl ?? "/"));
        var scope = "openid profile email User.Read";

        var authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
            $"?client_id={clientId}" +
            $"&response_type=code" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&response_mode=query";

        return Redirect(authUrl);
    }

    [HttpGet("microsoft-callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> MicrosoftCallback([FromQuery] string code, [FromQuery] string? state)
    {
        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];
        var tenantId = _configuration["AzureAd:TenantId"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/microsoft-callback";

        var returnUrl = "/";
        if (!string.IsNullOrEmpty(state))
        {
            try { returnUrl = Encoding.UTF8.GetString(Convert.FromBase64String(state)); }
            catch { /* ignore invalid state */ }
        }

        try
        {
            // Exchange code for tokens
            using var httpClient = new HttpClient();
            var tokenResponse = await httpClient.PostAsync(
                $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId!,
                    ["client_secret"] = clientSecret!,
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri,
                    ["grant_type"] = "authorization_code",
                    ["scope"] = "openid profile email User.Read",
                }));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Microsoft token exchange fallito: {Status}", tokenResponse.StatusCode);
                return Redirect($"/?error=microsoft_auth_failed");
            }

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<MicrosoftTokenResponse>();

            // Call Microsoft Graph API to get full user profile
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenData!.AccessToken);

            var graphUser = await httpClient.GetFromJsonAsync<MicrosoftGraphUser>(
                "https://graph.microsoft.com/v1.0/me");

            if (graphUser is null)
            {
                _logger.LogError("Impossibile ottenere profilo da Microsoft Graph");
                return Redirect($"/?error=microsoft_auth_failed");
            }

            var microsoftId = graphUser.Id;
            var email = graphUser.Mail ?? graphUser.UserPrincipalName;
            var givenName = graphUser.GivenName;
            var surname = graphUser.Surname;

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError("Email non trovata nel profilo Microsoft Graph");
                return Redirect($"/?error=microsoft_no_email");
            }

            // 1. Search by MicrosoftId (already linked)
            var user = _userManager.Users.FirstOrDefault(u => u.MicrosoftId == microsoftId);

            // 2. Search by email (auto-link)
            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(email);
                if (user is not null)
                {
                    // Auto-link: email matches existing account
                    user.MicrosoftId = microsoftId;
                    user.MicrosoftEmail = email;
                    user.DataCollegamentoMicrosoft = DateTime.UtcNow;
                    _logger.LogInformation("Account Microsoft collegato automaticamente per: {Email}", email);
                }
            }

            // 3. No match: auto-register new user
            if (user is null)
            {
                _logger.LogInformation("Auto-registrazione nuovo utente da Microsoft: {Email}", email);

                user = new ApplicationIdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Nome = givenName,
                    Cognome = surname,
                    Ruolo = "user",
                    MicrosoftId = microsoftId,
                    MicrosoftEmail = email,
                    DataCollegamentoMicrosoft = DateTime.UtcNow,
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("Errore creazione utente Microsoft: {Errors}", errors);
                    return Redirect($"/?error=microsoft_registration_failed");
                }

                await _userManager.AddToRoleAsync(user, "user");

                // Try to download Microsoft profile photo
                await TryDownloadMicrosoftPhoto(httpClient, user);
            }

            // Update login timestamps
            user.DataUltimoLogin = user.DataLogin;
            user.DataLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var appToken = await GenerateTokenResponse(user);

            _logger.LogInformation("Login Microsoft effettuato: {Email}", email);

            // Redirect to frontend callback page with token
            var accessToken = Uri.EscapeDataString(appToken.AccessToken);
            var userData = Uri.EscapeDataString(System.Text.Json.JsonSerializer.Serialize(appToken.UserData));
            var abilityRules = Uri.EscapeDataString(System.Text.Json.JsonSerializer.Serialize(appToken.UserAbilityRules));

            return Redirect($"/auth/microsoft-callback?accessToken={accessToken}&userData={userData}&userAbilityRules={abilityRules}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante callback Microsoft OAuth");
            return Redirect($"/?error=microsoft_auth_error");
        }
    }

    private async Task TryDownloadMicrosoftPhoto(HttpClient httpClient, ApplicationIdentityUser user)
    {
        try
        {
            var photoResponse = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");
            if (!photoResponse.IsSuccessStatusCode)
                return;

            var photoBytes = await photoResponse.Content.ReadAsByteArrayAsync();
            if (photoBytes.Length == 0)
                return;

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "avatars");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{user.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.jpg";
            var filePath = Path.Combine(uploadsDir, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, photoBytes);

            user.Foto = $"/uploads/avatars/{fileName}";
            _logger.LogInformation("Foto Microsoft scaricata per: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossibile scaricare foto Microsoft per: {Email}", user.Email);
        }
    }

    private async Task<TokenResponse> GenerateTokenResponse(ApplicationIdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Ruolo ?? "user";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email!),
            new(ClaimTypes.Email, user.Email!),
            new("nome", user.Nome ?? ""),
            new("cognome", user.Cognome ?? ""),
            new(ClaimTypes.Role, primaryRole),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "CoreERP-Development-Key-Change-In-Production-!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "120"));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Build ability rules based on role
        var abilityRules = BuildAbilityRules(primaryRole);

        return new TokenResponse
        {
            AccessToken = accessToken,
            UserData = new UserDataResponse
            {
                Id = user.Id,
                FullName = user.NomeCompleto,
                Email = user.Email!,
                Role = primaryRole,
                Avatar = user.Foto ?? "",
            },
            UserAbilityRules = abilityRules,
        };
    }

    private static List<AbilityRule> BuildAbilityRules(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "admin" => [new AbilityRule { Action = "manage", Subject = "all" }],
            _ =>
            [
                new AbilityRule { Action = "read", Subject = "dashboard" },
                new AbilityRule { Action = "read", Subject = "anagrafica" },
                new AbilityRule { Action = "read", Subject = "attivita" },
            ],
        };
    }
}

// Request/Response DTOs
public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record ForgotPasswordRequest
{
    public required string Email { get; init; }
}

public record ResetPasswordRequest
{
    public required string Email { get; init; }
    public required string Token { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}

public class TokenResponse
{
    public required string AccessToken { get; set; }
    public required UserDataResponse UserData { get; set; }
    public required List<AbilityRule> UserAbilityRules { get; set; }
}

public class UserDataResponse
{
    public required string Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public string Avatar { get; set; } = "";
}

public class AbilityRule
{
    public required string Action { get; set; }
    public required string Subject { get; set; }
}

public class MicrosoftTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class MicrosoftGraphUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("givenName")]
    public string? GivenName { get; set; }

    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("mail")]
    public string? Mail { get; set; }

    [JsonPropertyName("userPrincipalName")]
    public string? UserPrincipalName { get; set; }

    [JsonPropertyName("mobilePhone")]
    public string? MobilePhone { get; set; }

    [JsonPropertyName("jobTitle")]
    public string? JobTitle { get; set; }
}
