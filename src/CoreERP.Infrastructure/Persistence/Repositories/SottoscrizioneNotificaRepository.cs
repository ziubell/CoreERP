using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class SottoscrizioneNotificaRepository : ISottoscrizioneNotificaRepository
{
    private readonly ApplicationDbContext _context;

    public SottoscrizioneNotificaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFollowingAsync(string userId, string entitaTipo, int entitaId)
    {
        return await _context.SottoscrizioniNotifica
            .AnyAsync(s => s.UserId == userId && s.EntitaTipo == entitaTipo && s.EntitaId == entitaId);
    }

    public async Task FollowAsync(string userId, string entitaTipo, int entitaId)
    {
        var exists = await IsFollowingAsync(userId, entitaTipo, entitaId);
        if (exists) return;

        _context.SottoscrizioniNotifica.Add(new SottoscrizioneNotifica
        {
            UserId = userId,
            EntitaTipo = entitaTipo,
            EntitaId = entitaId,
            DataSottoscrizione = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task UnfollowAsync(string userId, string entitaTipo, int entitaId)
    {
        await _context.SottoscrizioniNotifica
            .Where(s => s.UserId == userId && s.EntitaTipo == entitaTipo && s.EntitaId == entitaId)
            .ExecuteDeleteAsync();
    }

    public async Task<List<string>> GetFollowersAsync(string entitaTipo, int entitaId)
    {
        return await _context.SottoscrizioniNotifica
            .Where(s => s.EntitaTipo == entitaTipo && s.EntitaId == entitaId)
            .Select(s => s.UserId)
            .ToListAsync();
    }

    public async Task<List<SottoscrizioneNotifica>> GetByUserAsync(string userId, string? entitaTipo = null)
    {
        var query = _context.SottoscrizioniNotifica
            .Where(s => s.UserId == userId);

        if (entitaTipo is not null)
            query = query.Where(s => s.EntitaTipo == entitaTipo);

        return await query
            .OrderByDescending(s => s.DataSottoscrizione)
            .ToListAsync();
    }

    public async Task<SottoscrizioneNotifica?> GetAsync(string userId, string entitaTipo, int entitaId)
    {
        return await _context.SottoscrizioniNotifica
            .FirstOrDefaultAsync(s => s.UserId == userId && s.EntitaTipo == entitaTipo && s.EntitaId == entitaId);
    }

    public async Task UpdateAsync(SottoscrizioneNotifica sottoscrizione)
    {
        _context.Entry(sottoscrizione).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
