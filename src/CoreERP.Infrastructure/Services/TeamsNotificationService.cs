using System.Text;
using System.Text.Json;
using CoreERP.Application.Interfaces;
using CoreERP.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Services;

public class TeamsNotificationService : ITeamsNotificationService
{
    private readonly IMicrosoftGraphService _graphService;
    private readonly ILogger<TeamsNotificationService> _logger;

    public TeamsNotificationService(
        IMicrosoftGraphService graphService,
        ILogger<TeamsNotificationService> logger)
    {
        _graphService = graphService;
        _logger = logger;
    }

    public async Task InviaAsync(string userId, string titolo, string? messaggio = null, string? link = null)
    {
        var client = await _graphService.CreateGraphClientAsync(userId);
        if (client is null)
        {
            _logger.LogDebug("Notifica Teams non inviata: utente {UserId} non ha Microsoft collegato", userId);
            return;
        }

        try
        {
            var chatMessage = new
            {
                body = new
                {
                    contentType = "html",
                    content = BuildTeamsMessage(titolo, messaggio, link)
                }
            };

            // Send as a chat message to the user via Microsoft Graph
            // Using /me/chats to send activity feed notification
            var payload = JsonSerializer.Serialize(chatMessage);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            // Send activity feed notification
            var activityPayload = new
            {
                topic = new
                {
                    source = "text",
                    value = "CoreERP",
                    webUrl = link ?? "https://coreerp.spadhausen.com"
                },
                activityType = "taskCreated",
                previewText = new { content = titolo }
            };

            var activityJson = JsonSerializer.Serialize(activityPayload);
            var activityContent = new StringContent(activityJson, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://graph.microsoft.com/v1.0/me/teamwork/sendActivityNotification",
                activityContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notifica Teams inviata a utente {UserId}", userId);
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Notifica Teams fallita per utente {UserId}: {Status} - {Body}",
                    userId, response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore invio notifica Teams a utente {UserId}", userId);
        }
    }

    private static string BuildTeamsMessage(string titolo, string? messaggio, string? link)
    {
        var sb = new StringBuilder();
        sb.Append($"<strong>{titolo}</strong>");

        if (!string.IsNullOrEmpty(messaggio))
            sb.Append($"<br/>{messaggio}");

        if (!string.IsNullOrEmpty(link))
            sb.Append($"<br/><a href=\"{link}\">Apri in CoreERP</a>");

        return sb.ToString();
    }
}
