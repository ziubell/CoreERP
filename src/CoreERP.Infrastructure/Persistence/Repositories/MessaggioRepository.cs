using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Messaggi;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class MessaggioRepository : IMessaggioRepository
{
    private readonly ApplicationDbContext _context;

    public MessaggioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Messaggio>> GetByEntitaAsync(string entitaTipo, int entitaId,
        int pagina = 1, int dimensionePagina = 20)
    {
        return await _context.Messaggi
            .Include(m => m.Allegati)
            .Where(m => m.EntitaTipo == entitaTipo && m.EntitaId == entitaId)
            .OrderByDescending(m => m.DataCreazione)
            .Skip((pagina - 1) * dimensionePagina)
            .Take(dimensionePagina)
            .ToListAsync();
    }

    public async Task<int> CountByEntitaAsync(string entitaTipo, int entitaId)
    {
        return await _context.Messaggi
            .CountAsync(m => m.EntitaTipo == entitaTipo && m.EntitaId == entitaId);
    }

    public async Task<Messaggio?> GetByIdAsync(int id)
    {
        return await _context.Messaggi
            .Include(m => m.Allegati)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<AllegatoMessaggio?> GetAllegatoByIdAsync(int allegatoId)
    {
        return await _context.AllegatiMessaggio
            .FirstOrDefaultAsync(a => a.Id == allegatoId);
    }

    public async Task<Messaggio> AddAsync(Messaggio messaggio)
    {
        _context.Messaggi.Add(messaggio);
        await _context.SaveChangesAsync();
        return messaggio;
    }

    public async Task UpdateAsync(Messaggio messaggio)
    {
        _context.Messaggi.Update(messaggio);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var messaggio = await _context.Messaggi
            .Include(m => m.Allegati)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (messaggio != null)
        {
            _context.Messaggi.Remove(messaggio);
            await _context.SaveChangesAsync();
        }
    }
}
