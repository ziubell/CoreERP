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
        // Wait for the application to fully start before first run
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PulisciNotificheAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Application is shutting down, exit gracefully
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la pulizia delle notifiche");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task PulisciNotificheAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

        var count2 = await context.Notifiche
            .Where(n => !userIdsWithSettings.Contains(n.UserId) && n.DataCreazione < defaultCutoff)
            .ExecuteDeleteAsync(ct);

        totalEliminate += count2;

        if (totalEliminate > 0)
            _logger.LogInformation("Pulizia notifiche completata: {Count} notifiche eliminate", totalEliminate);
    }
}
