using CoreERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Services;

public class NotificaCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificaCleanupService> _logger;
    private const int DefaultGiorniRetention = 90;

    public NotificaCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificaCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PulisciNotificheAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la pulizia delle notifiche");
            }

            // Run once per day
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task PulisciNotificheAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get all user retention settings
        var impostazioni = await context.ImpostazioniNotificaUtente
            .Where(i => i.GiorniRetention > 0)
            .ToListAsync(ct);

        var totalEliminate = 0;

        foreach (var imp in impostazioni)
        {
            var cutoff = DateTime.UtcNow.AddDays(-imp.GiorniRetention);
            var count = await context.Notifiche
                .Where(n => n.UserId == imp.UserId && n.DataCreazione < cutoff)
                .ExecuteDeleteAsync(ct);

            totalEliminate += count;
        }

        // For users without custom settings, apply default retention
        var userIdsWithSettings = impostazioni.Select(i => i.UserId).ToHashSet();
        var defaultCutoff = DateTime.UtcNow.AddDays(-DefaultGiorniRetention);

        var userIdsSenzaImpostazioni = await context.Notifiche
            .Where(n => !userIdsWithSettings.Contains(n.UserId) && n.DataCreazione < defaultCutoff)
            .Select(n => n.UserId)
            .Distinct()
            .ToListAsync(ct);

        if (userIdsSenzaImpostazioni.Count > 0)
        {
            var count = await context.Notifiche
                .Where(n => !userIdsWithSettings.Contains(n.UserId) && n.DataCreazione < defaultCutoff)
                .ExecuteDeleteAsync(ct);

            totalEliminate += count;
        }

        if (totalEliminate > 0)
            _logger.LogInformation("Pulizia notifiche completata: {Count} notifiche eliminate", totalEliminate);
    }
}
