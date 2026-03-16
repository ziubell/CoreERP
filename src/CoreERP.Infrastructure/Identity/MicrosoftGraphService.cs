using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Identity;

public interface IMicrosoftGraphService
{
    /// <summary>
    /// Gets a valid Microsoft access token for the user, refreshing if expired.
    /// Returns null if the user has no Microsoft account linked or refresh fails.
    /// </summary>
    Task<string?> GetAccessTokenAsync(string userId);

    /// <summary>
    /// Creates an HttpClient with the user's Microsoft access token set in the Authorization header.
    /// Returns null if no valid token is available.
    /// </summary>
    Task<HttpClient?> CreateGraphClientAsync(string userId);
}

public class MicrosoftGraphService : IMicrosoftGraphService
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MicrosoftGraphService> _logger;

    public MicrosoftGraphService(
        UserManager<ApplicationIdentityUser> userManager,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<MicrosoftGraphService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> GetAccessTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || string.IsNullOrEmpty(user.MicrosoftRefreshToken))
            return null;

        // Token still valid (with 5 min buffer)
        if (user.MicrosoftTokenExpiry.HasValue
            && user.MicrosoftTokenExpiry.Value > DateTime.UtcNow.AddMinutes(5)
            && !string.IsNullOrEmpty(user.MicrosoftAccessToken))
        {
            return user.MicrosoftAccessToken;
        }

        // Token expired or about to expire, refresh it
        return await RefreshTokenAsync(user);
    }

    public async Task<HttpClient?> CreateGraphClientAsync(string userId)
    {
        var accessToken = await GetAccessTokenAsync(userId);
        if (accessToken is null)
            return null;

        var client = _httpClientFactory.CreateClient("MicrosoftGraph");
        client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }

    private async Task<string?> RefreshTokenAsync(ApplicationIdentityUser user)
    {
        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];
        var tenantId = _configuration["AzureAd:TenantId"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogWarning("Azure AD non configurato, impossibile rinnovare token per: {UserId}", user.Id);
            return null;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(
                $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["refresh_token"] = user.MicrosoftRefreshToken!,
                    ["grant_type"] = "refresh_token",
                    ["scope"] = "openid profile email User.Read offline_access",
                }));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Refresh token Microsoft fallito per {UserId}: {Status} - {Body}",
                    user.Id, response.StatusCode, errorBody);

                // If refresh token is revoked/expired, clear Microsoft tokens
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    user.MicrosoftAccessToken = null;
                    user.MicrosoftRefreshToken = null;
                    user.MicrosoftTokenExpiry = null;
                    await _userManager.UpdateAsync(user);
                    _logger.LogWarning("Token Microsoft invalidati per {UserId}, refresh token scaduto/revocato", user.Id);
                }

                return null;
            }

            var tokenData = await response.Content.ReadFromJsonAsync<MicrosoftRefreshTokenResponse>();

            user.MicrosoftAccessToken = tokenData!.AccessToken;
            user.MicrosoftTokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

            // Microsoft may return a new refresh token (rotation)
            if (!string.IsNullOrEmpty(tokenData.RefreshToken))
                user.MicrosoftRefreshToken = tokenData.RefreshToken;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token Microsoft rinnovato per: {UserId}", user.Id);
            return user.MicrosoftAccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante refresh token Microsoft per: {UserId}", user.Id);
            return null;
        }
    }
}

internal class MicrosoftRefreshTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
