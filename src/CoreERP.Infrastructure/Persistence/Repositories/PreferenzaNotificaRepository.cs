using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class PreferenzaNotificaRepository : IPreferenzaNotificaRepository
{
    private readonly ApplicationDbContext _context;

    public PreferenzaNotificaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PreferenzaNotificaUtente>> GetByUserAsync(string userId)
    {
        return await _context.PreferenzeNotificaUtente
            .Include(p => p.TipoNotifica)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<PreferenzaNotificaUtente?> GetByUserAndTipoAsync(string userId, int tipoNotificaId)
    {
        return await _context.PreferenzeNotificaUtente
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TipoNotificaId == tipoNotificaId);
    }

    public async Task SalvaPreferenzeAsync(string userId, List<PreferenzaNotificaDto> preferenze)
    {
        foreach (var pref in preferenze)
        {
            var existing = await _context.PreferenzeNotificaUtente
                .FirstOrDefaultAsync(p => p.UserId == userId && p.TipoNotificaId == pref.TipoNotificaId);

            if (existing is not null)
            {
                existing.Email = pref.Email;
                existing.Browser = pref.Browser;
                existing.Teams = pref.Teams;
            }
            else
            {
                _context.PreferenzeNotificaUtente.Add(new PreferenzaNotificaUtente
                {
                    UserId = userId,
                    TipoNotificaId = pref.TipoNotificaId,
                    Email = pref.Email,
                    Browser = pref.Browser,
                    Teams = pref.Teams
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
