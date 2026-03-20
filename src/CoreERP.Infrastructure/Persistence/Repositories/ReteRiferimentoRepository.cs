using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class ReteRiferimentoRepository : IReteRiferimentoRepository
{
    private readonly ApplicationDbContext _context;

    public ReteRiferimentoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReteRiferimento>> GetAllAsync(bool? attivo = null)
    {
        var query = _context.RetiRiferimento.AsQueryable();
        if (attivo.HasValue)
            query = query.Where(r => r.Attivo == attivo.Value);
        return await query.OrderBy(r => r.Ordine).ThenBy(r => r.Nome).ToListAsync();
    }

    public async Task<ReteRiferimento?> GetByIdAsync(int id)
        => await _context.RetiRiferimento.FindAsync(id);

    public async Task<ReteRiferimento> AddAsync(ReteRiferimento entity)
    {
        _context.RetiRiferimento.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ReteRiferimento entity)
    {
        _context.RetiRiferimento.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        var entity = await _context.RetiRiferimento.FindAsync(id);
        if (entity == null) return false;
        return await _context.Indirizzi.AnyAsync(i => i.Rete == entity.Nome);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.RetiRiferimento.FindAsync(id);
        if (entity != null)
        {
            _context.RetiRiferimento.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
