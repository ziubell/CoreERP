using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class NotificaRepository : INotificaRepository
{
    private readonly ApplicationDbContext _context;

    public NotificaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notifica>> GetByUserAsync(string userId, bool soloNonLette = false,
        int pagina = 1, int dimensionePagina = 20,
        string? ricerca = null, string? modulo = null)
    {
        var query = _context.Notifiche
            .Include(n => n.TipoNotifica)
            .Where(n => n.UserId == userId);

        if (soloNonLette)
            query = query.Where(n => !n.Letta);

        if (!string.IsNullOrWhiteSpace(ricerca))
        {
            var term = ricerca.Trim();
            query = query.Where(n => n.Titolo.Contains(term) || (n.Messaggio != null && n.Messaggio.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(modulo))
            query = query.Where(n => n.TipoNotifica.Modulo == modulo);

        return await query
            .OrderByDescending(n => n.DataCreazione)
            .Skip((pagina - 1) * dimensionePagina)
            .Take(dimensionePagina)
            .ToListAsync();
    }

    public async Task<int> ContaNonLetteAsync(string userId)
    {
        return await _context.Notifiche
            .CountAsync(n => n.UserId == userId && !n.Letta);
    }

    public async Task SegnaComeLettaAsync(int notificaId, string userId)
    {
        var notifica = await _context.Notifiche
            .FirstOrDefaultAsync(n => n.Id == notificaId && n.UserId == userId);

        if (notifica is not null)
        {
            notifica.Letta = true;
            notifica.DataLettura = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SegnaTutteComeLettaAsync(string userId)
    {
        await _context.Notifiche
            .Where(n => n.UserId == userId && !n.Letta)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Letta, true)
                .SetProperty(n => n.DataLettura, DateTime.UtcNow));
    }

    public async Task EliminaAsync(int notificaId, string userId)
    {
        await _context.Notifiche
            .Where(n => n.Id == notificaId && n.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task EliminaMultipleAsync(List<int> ids, string userId)
    {
        await _context.Notifiche
            .Where(n => ids.Contains(n.Id) && n.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task EliminaTutteAsync(string userId)
    {
        await _context.Notifiche
            .Where(n => n.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task<Notifica> AddAsync(Notifica notifica)
    {
        _context.Notifiche.Add(notifica);
        await _context.SaveChangesAsync();
        return notifica;
    }
}
