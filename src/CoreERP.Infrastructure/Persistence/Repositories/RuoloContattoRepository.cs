using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class RuoloContattoRepository : IRuoloContattoRepository
{
    private readonly ApplicationDbContext _context;

    public RuoloContattoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RuoloContatto>> GetAllAsync(bool? attivo = null)
    {
        var query = _context.RuoliContatto.AsQueryable();
        if (attivo.HasValue)
            query = query.Where(r => r.Attivo == attivo.Value);
        return await query.OrderBy(r => r.Ordine).ThenBy(r => r.Nome).ToListAsync();
    }

    public async Task<RuoloContatto?> GetByIdAsync(int id)
        => await _context.RuoliContatto.FindAsync(id);

    public async Task<RuoloContatto> AddAsync(RuoloContatto ruolo)
    {
        _context.RuoliContatto.Add(ruolo);
        await _context.SaveChangesAsync();
        return ruolo;
    }

    public async Task UpdateAsync(RuoloContatto ruolo)
    {
        _context.RuoliContatto.Update(ruolo);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsInUseAsync(int id)
        => await _context.AnagraficaContatti.AnyAsync(ac => ac.RuoloContattoId == id);

    public async Task DeleteAsync(int id)
    {
        var ruolo = await _context.RuoliContatto.FindAsync(id);
        if (ruolo != null)
        {
            _context.RuoliContatto.Remove(ruolo);
            await _context.SaveChangesAsync();
        }
    }
}
