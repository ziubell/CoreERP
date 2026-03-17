using System.Text;
using System.Text.Json;
using CoreERP.Application.Interfaces;
using CoreERP.Infrastructure.Configuration;
using CoreERP.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreERP.Infrastructure.Services;

public class TeamsNotificationService : ITeamsNotificationService
{
    private const string GraphBaseUrl = "https://graph.microsoft.com/v1.0";

    private readonly IMicrosoftGraphService _graphService;
    private readonly TeamsOptions _teamsOptions;
    private readonly ILogger<TeamsNotificationService> _logger;

    public TeamsNotificationService(
        IMicrosoftGraphService graphService,
        IOptions<TeamsOptions> teamsOptions,
        ILogger<TeamsNotificationService> logger)
    {
        _graphService = graphService;
        _teamsOptions = teamsOptions.Value;
        _logger = logger;
    }

    public async Task InviaAsync(string userId, string titolo, string? messaggio = null, string? link = null)
    {
        if (!_teamsOptions.Enabled)
        {
            _logger.LogDebug("Modulo Teams non attivo, notifica non inviata");
            return;
        }

        var userMsId = await _graphService.GetUserMicrosoftIdAsync(userId);
        if (string.IsNullOrEmpty(userMsId))
        {
            _logger.LogDebug("Notifica Teams non inviata: utente {UserId} non ha Microsoft collegato", userId);
            return;
        }

        await SendActivityFeedAsync(userMsId, userId, titolo, messaggio, link);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Activity Feed (app-only — TeamsActivity.Send)
    // ══════════════════════════════════════════════════════════════════════

    private async Task SendActivityFeedAsync(
        string userMsId, string userId, string titolo, string? messaggio, string? link)
    {
        try
        {
            var appClient = await _graphService.CreateAppGraphClientAsync();
            if (appClient is null)
            {
                _logger.LogWarning("Notifica Teams non inviata a {UserId}: impossibile ottenere app token", userId);
                return;
            }

            await EnsureTeamsAppInstalledAsync(appClient, userMsId);

            var catalogAppId = _teamsOptions.TeamsCatalogAppId;
            var deepLink = $"https://teams.microsoft.com/l/entity/{catalogAppId}/notifications";
            if (!string.IsNullOrWhiteSpace(link))
            {
                deepLink += $"?webUrl={Uri.EscapeDataString(link)}";
            }

            var payload = new
            {
                topic = new
                {
                    source = "text",
                    value = "CoreERP",
                    webUrl = deepLink
                },
                activityType = "coreErpNotifica",
                previewText = new { content = string.IsNullOrWhiteSpace(messaggio) ? titolo : $"{titolo} - {messaggio}" },
                templateParameters = new[]
                {
                    new { name = "titolo", value = titolo }
                }
            };

            var response = await appClient.PostAsync(
                $"{GraphBaseUrl}/users/{userMsId}/teamwork/sendActivityNotification",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notifica Activity Feed inviata a {UserId}", userId);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Activity Feed fallito ({Status}) per {UserId}: {Body}",
                    response.StatusCode, userId, errorBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore invio notifica Activity Feed a {UserId}", userId);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Teams app installation
    // ══════════════════════════════════════════════════════════════════════

    private async Task EnsureTeamsAppInstalledAsync(HttpClient appClient, string userMsId)
    {
        var catalogAppId = _teamsOptions.TeamsCatalogAppId;
        if (string.IsNullOrWhiteSpace(catalogAppId))
            return;

        try
        {
            var checkResponse = await appClient.GetAsync(
                $"{GraphBaseUrl}/users/{userMsId}/teamwork/installedApps?$filter=teamsApp/id eq '{catalogAppId}'&$select=id");

            if (checkResponse.IsSuccessStatusCode)
            {
                var json = await checkResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var values = doc.RootElement.GetProperty("value");
                if (values.GetArrayLength() > 0)
                    return;
            }

            var installJson = $"{{\"teamsApp@odata.bind\":\"{GraphBaseUrl}/appCatalogs/teamsApps/{catalogAppId}\"}}";

            var installResponse = await appClient.PostAsync(
                $"{GraphBaseUrl}/users/{userMsId}/teamwork/installedApps",
                new StringContent(installJson, Encoding.UTF8, "application/json"));

            if (installResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("App Teams installata per utente {UserMsId}", userMsId);
            }
            else
            {
                var errBody = await installResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Installazione app Teams fallita per {UserMsId} ({Status}): {Body}",
                    userMsId, installResponse.StatusCode, errBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore verifica/installazione app Teams per {UserMsId}", userMsId);
        }
    }
}
