using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class IndirizzoRepository : IIndirizzoRepository
{
    private readonly ApplicationDbContext _db;

    public IndirizzoRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Indirizzo>> GetByAnagraficaIdAsync(int anagraficaId)
    {
        return await _db.Indirizzi
            .Where(i => i.AnagraficaId == anagraficaId)
            .OrderBy(i => i.Tipo)
            .ThenByDescending(i => i.Principale)
            .ToListAsync();
    }

    public async Task<List<Indirizzo>> GetAllAsync(string? tipo = null, string? ricerca = null, int pagina = 1, int dimensionePagina = 20)
    {
        var query = _db.Indirizzi.Include(i => i.Anagrafica).AsQueryable();

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(i => i.Tipo == tipo);

        if (!string.IsNullOrEmpty(ricerca))
            query = query.Where(i =>
                i.Strada.Contains(ricerca) ||
                i.Citta.Contains(ricerca) ||
                i.Anagrafica.Denominazione.Contains(ricerca));

        return await query
            .OrderByDescending(i => i.DataCreazione)
            .Skip((pagina - 1) * dimensionePagina)
            .Take(dimensionePagina)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? tipo = null, string? ricerca = null)
    {
        var query = _db.Indirizzi.AsQueryable();

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(i => i.Tipo == tipo);

        if (!string.IsNullOrEmpty(ricerca))
            query = query.Where(i =>
                i.Strada.Contains(ricerca) ||
                i.Citta.Contains(ricerca) ||
                i.Anagrafica.Denominazione.Contains(ricerca));

        return await query.CountAsync();
    }

    public async Task<Indirizzo?> GetByIdAsync(int id)
    {
        return await _db.Indirizzi
            .Include(i => i.Anagrafica)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Indirizzo> AddAsync(Indirizzo indirizzo)
    {
        _db.Indirizzi.Add(indirizzo);
        await _db.SaveChangesAsync();
        return indirizzo;
    }

    public async Task UpdateAsync(Indirizzo indirizzo)
    {
        _db.Entry(indirizzo).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var indirizzo = await _db.Indirizzi.FindAsync(id);
        if (indirizzo != null)
        {
            _db.Indirizzi.Remove(indirizzo);
            await _db.SaveChangesAsync();
        }
    }
}
