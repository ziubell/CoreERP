using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Serialization;
using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserManager<ApplicationIdentityUser> userManager,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new ProfileResponse
        {
            Id = user.Id,
            Nome = user.Nome ?? "",
            Cognome = user.Cognome ?? "",
            Email = user.Email ?? "",
            Cellulare = user.Cellulare ?? "",
            Ruolo = user.Ruolo ?? roles.FirstOrDefault() ?? "user",
            Foto = user.Foto ?? "",
            Dipendente = user.Dipendente,
            Reperibile = user.Reperibile,
            CodiceAgente = user.CodiceAgente ?? "",
            DataLogin = user.DataLogin,
            DataUltimoLogin = user.DataUltimoLogin,
            MicrosoftLinked = !string.IsNullOrEmpty(user.MicrosoftId),
            MicrosoftEmail = user.MicrosoftEmail,
            DataCollegamentoMicrosoft = user.DataCollegamentoMicrosoft,
        });
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Unauthorized();

        user.Nome = request.Nome?.Trim();
        user.Cognome = request.Cognome?.Trim();
        user.Cellulare = request.Cellulare?.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Errore nell'aggiornamento del profilo." });
        }

        _logger.LogInformation("Profilo aggiornato per: {Email}", user.Email);

        // Return updated userData for cookie sync
        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Ruolo ?? "user";

        return Ok(new
        {
            message = "Profilo aggiornato con successo.",
            userData = new UserDataResponse
            {
                Id = user.Id,
                FullName = user.NomeCompleto,
                Email = user.Email!,
                Role = primaryRole,
                Avatar = user.Foto ?? "",
            }
        });
    }

    /// <summary>
    /// Upload profile photo
    /// </summary>
    [HttpPost("foto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Nessun file selezionato." });

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(new { message = "Formato non supportato. Usa JPG, PNG o WebP." });

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File troppo grande. Massimo 5MB." });

        // Create upload directory
        var uploadsDir = Path.Combine(_environment.ContentRootPath, "uploads", "avatars");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        // Delete old photo if exists
        if (!string.IsNullOrEmpty(user.Foto))
        {
            var oldPath = Path.Combine(_environment.ContentRootPath, user.Foto.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        // Save new photo
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            extension = file.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };

        var fileName = $"{user.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Update user photo path
        user.Foto = $"/uploads/avatars/{fileName}";
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Foto profilo aggiornata per: {Email}", user.Email);

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Ruolo ?? "user";

        return Ok(new
        {
            message = "Foto aggiornata con successo.",
            avatar = user.Foto,
            userData = new UserDataResponse
            {
                Id = user.Id,
                FullName = user.NomeCompleto,
                Email = user.Email!,
                Role = primaryRole,
                Avatar = user.Foto,
            }
        });
    }

    /// <summary>
    /// Delete profile photo
    /// </summary>
    [HttpDelete("foto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePhoto()
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Unauthorized();

        if (!string.IsNullOrEmpty(user.Foto))
        {
            var oldPath = Path.Combine(_environment.ContentRootPath, user.Foto.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        user.Foto = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Foto profilo rimossa per: {Email}", user.Email);

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Ruolo ?? "user";

        return Ok(new
        {
            message = "Foto rimossa con successo.",
            userData = new UserDataResponse
            {
                Id = user.Id,
                FullName = user.NomeCompleto,
                Email = user.Email!,
                Role = primaryRole,
                Avatar = "",
            }
        });
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Unauthorized();

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { message = "Le password non corrispondono." });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Cambio password fallito per {Email}: {Errors}", user.Email, errors);
            return BadRequest(new { message = "Password attuale non corretta o nuova password non valida." });
        }

        _logger.LogInformation("Password cambiata per: {Email}", user.Email);

        return Ok(new { message = "Password aggiornata con successo." });
    }

    /// <summary>
    /// Get Microsoft account link status
    /// </summary>
    [HttpGet("microsoft-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMicrosoftStatus()
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Unauthorized();

        var hasPassword = await _userManager.HasPasswordAsync(user);

        return Ok(new MicrosoftStatusResponse
        {
            IsLinked = !string.IsNullOrEmpty(user.MicrosoftId),
            MicrosoftEmail = user.MicrosoftEmail,
            DataCollegamento = user.DataCollegamentoMicrosoft,
            HasPassword = hasPassword,
        });
    }

    /// <summary>
    /// Start Microsoft account linking flow
    /// </summary>
    [HttpGet("microsoft-link")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult MicrosoftLink()
    {
        var clientId = _configuration["AzureAd:ClientId"];
        if (string.IsNullOrEmpty(clientId))
            return BadRequest(new { message = "Autenticazione Microsoft non configurata." });

        var tenantId = _configuration["AzureAd:TenantId"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/profile/microsoft-link-callback";
        var scope = "openid profile email User.Read offline_access";

        var authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
            $"?client_id={clientId}" +
            $"&response_type=code" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            $"&response_mode=query";

        return Redirect(authUrl);
    }

    /// <summary>
    /// Microsoft account linking callback
    /// </summary>
    [HttpGet("microsoft-link-callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> MicrosoftLinkCallback([FromQuery] string code)
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Redirect("/account-settings?tab=security&microsoft=auth_required");

        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];
        var tenantId = _configuration["AzureAd:TenantId"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/profile/microsoft-link-callback";

        try
        {
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
                    ["scope"] = "openid profile email User.Read offline_access",
                }));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Microsoft token exchange fallito durante linking");
                return Redirect("/account-settings?tab=security&microsoft=link_failed");
            }

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<MicrosoftTokenResponse>();

            // Get Microsoft profile via Graph API
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenData!.AccessToken);

            var graphUser = await httpClient.GetFromJsonAsync<MicrosoftGraphUser>(
                "https://graph.microsoft.com/v1.0/me");

            if (graphUser is null || string.IsNullOrEmpty(graphUser.Id))
            {
                return Redirect("/account-settings?tab=security&microsoft=link_failed");
            }

            // Check if this Microsoft account is already linked to another user
            var existingUser = _userManager.Users.FirstOrDefault(u => u.MicrosoftId == graphUser.Id && u.Id != user.Id);
            if (existingUser is not null)
            {
                _logger.LogWarning("Account Microsoft già collegato a un altro utente: {MsId}", graphUser.Id);
                return Redirect("/account-settings?tab=security&microsoft=already_linked_other");
            }

            // Link Microsoft account and save tokens
            user.MicrosoftId = graphUser.Id;
            user.MicrosoftEmail = graphUser.Mail ?? graphUser.UserPrincipalName;
            user.DataCollegamentoMicrosoft = DateTime.UtcNow;
            user.MicrosoftAccessToken = tokenData.AccessToken;
            user.MicrosoftRefreshToken = tokenData.RefreshToken;
            user.MicrosoftTokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Account Microsoft collegato per: {Email}", user.Email);

            return Redirect("/account-settings?tab=security&microsoft=linked");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante linking Microsoft");
            return Redirect("/account-settings?tab=security&microsoft=link_failed");
        }
    }

    /// <summary>
    /// Unlink Microsoft account
    /// </summary>
    [HttpDelete("microsoft-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlinkMicrosoft()
    {
        var user = await GetCurrentUser();
        if (user is null)
            return Unauthorized();

        if (string.IsNullOrEmpty(user.MicrosoftId))
            return BadRequest(new { message = "Nessun account Microsoft collegato." });

        // Verify user has a password before unlinking
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
            return BadRequest(new { message = "Devi prima impostare una password prima di scollegare l'account Microsoft." });

        user.MicrosoftId = null;
        user.MicrosoftEmail = null;
        user.DataCollegamentoMicrosoft = null;
        user.MicrosoftAccessToken = null;
        user.MicrosoftRefreshToken = null;
        user.MicrosoftTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Account Microsoft scollegato per: {Email}", user.Email);

        return Ok(new { message = "Account Microsoft scollegato con successo." });
    }

    private async Task<ApplicationIdentityUser?> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null;

        return await _userManager.FindByIdAsync(userId);
    }
}

// DTOs
public class ProfileResponse
{
    public required string Id { get; set; }
    public string Nome { get; set; } = "";
    public string Cognome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Cellulare { get; set; } = "";
    public string Ruolo { get; set; } = "";
    public string Foto { get; set; } = "";
    public bool Dipendente { get; set; }
    public bool Reperibile { get; set; }
    public string CodiceAgente { get; set; } = "";
    public DateTime? DataLogin { get; set; }
    public DateTime? DataUltimoLogin { get; set; }
    public bool MicrosoftLinked { get; set; }
    public string? MicrosoftEmail { get; set; }
    public DateTime? DataCollegamentoMicrosoft { get; set; }
}

public class MicrosoftStatusResponse
{
    public bool IsLinked { get; set; }
    public string? MicrosoftEmail { get; set; }
    public DateTime? DataCollegamento { get; set; }
    public bool HasPassword { get; set; }
}

public record UpdateProfileRequest
{
    public string? Nome { get; init; }
    public string? Cognome { get; init; }
    public string? Cellulare { get; init; }
}

public record ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
    public required string ConfirmPassword { get; init; }
}
