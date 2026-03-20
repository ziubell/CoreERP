using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class TipoTecnologiaRepository : ITipoTecnologiaRepository
{
    private readonly ApplicationDbContext _context;

    public TipoTecnologiaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TipoTecnologia>> GetAllAsync(bool? attivo = null)
    {
        var query = _context.TipiTecnologia.AsQueryable();
        if (attivo.HasValue)
            query = query.Where(t => t.Attivo == attivo.Value);
        return await query.OrderBy(t => t.Ordine).ThenBy(t => t.Nome).ToListAsync();
    }

    public async Task<TipoTecnologia?> GetByIdAsync(int id)
        => await _context.TipiTecnologia.FindAsync(id);

    public async Task<TipoTecnologia> AddAsync(TipoTecnologia entity)
    {
        _context.TipiTecnologia.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TipoTecnologia entity)
    {
        _context.TipiTecnologia.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        var entity = await _context.TipiTecnologia.FindAsync(id);
        if (entity == null) return false;
        return await _context.Indirizzi.AnyAsync(i => i.SottoTipo == entity.Nome);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.TipiTecnologia.FindAsync(id);
        if (entity != null)
        {
            _context.TipiTecnologia.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
