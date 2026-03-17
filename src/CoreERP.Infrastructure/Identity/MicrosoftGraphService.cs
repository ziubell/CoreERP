using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Identity;

public interface IMicrosoftGraphService
{
    /// <summary>
    /// Creates an HttpClient using an app-only token (client credentials flow).
    /// Used for application-level Graph API calls (e.g., activity feed with TeamsActivity.Send).
    /// </summary>
    Task<HttpClient?> CreateAppGraphClientAsync();

    /// <summary>
    /// Returns the MicrosoftId (Azure AD Object ID) for the given application user.
    /// </summary>
    Task<string?> GetUserMicrosoftIdAsync(string userId);
}

public class MicrosoftGraphService : IMicrosoftGraphService
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MicrosoftGraphService> _logger;

    private string? _appAccessToken;
    private DateTime _appTokenExpiry = DateTime.MinValue;

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

    public async Task<string?> GetUserMicrosoftIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.MicrosoftId;
    }

    public async Task<HttpClient?> CreateAppGraphClientAsync()
    {
        var accessToken = await GetAppAccessTokenAsync();
        if (accessToken is null)
            return null;

        var client = _httpClientFactory.CreateClient("MicrosoftGraph");
        client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }

    private async Task<string?> GetAppAccessTokenAsync()
    {
        if (_appAccessToken is not null && _appTokenExpiry > DateTime.UtcNow.AddMinutes(5))
            return _appAccessToken;

        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];
        var tenantId = _configuration["AzureAd:TenantId"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("Azure AD non configurato, impossibile ottenere app token");
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
                    ["grant_type"] = "client_credentials",
                    ["scope"] = "https://graph.microsoft.com/.default",
                }));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Client credentials token fallito: {Status} - {Body}",
                    response.StatusCode, errorBody);
                return null;
            }

            var tokenData = await response.Content.ReadFromJsonAsync<AppTokenResponse>();
            _appAccessToken = tokenData!.AccessToken;
            _appTokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

            _logger.LogInformation("App token (client credentials) ottenuto con successo");
            return _appAccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante ottenimento app token (client credentials)");
            return null;
        }
    }
}

internal class AppTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
