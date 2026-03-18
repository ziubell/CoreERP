using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class MetodoPagamentoRepository : IMetodoPagamentoRepository
{
    private readonly ApplicationDbContext _context;

    public MetodoPagamentoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MetodoPagamento>> GetAllAsync(bool? attivo = null)
    {
        var query = _context.MetodiPagamento.AsQueryable();
        if (attivo.HasValue)
            query = query.Where(m => m.Attivo == attivo.Value);
        return await query.OrderBy(m => m.Ordine).ThenBy(m => m.Nome).ToListAsync();
    }

    public async Task<MetodoPagamento?> GetByIdAsync(int id)
        => await _context.MetodiPagamento.FindAsync(id);

    public async Task<MetodoPagamento> AddAsync(MetodoPagamento metodo)
    {
        _context.MetodiPagamento.Add(metodo);
        await _context.SaveChangesAsync();
        return metodo;
    }

    public async Task UpdateAsync(MetodoPagamento metodo)
    {
        _context.MetodiPagamento.Update(metodo);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsInUseAsync(int id)
        => await _context.Anagrafiche.AnyAsync(a => a.MetodoPagamentoId == id);

    public async Task DeleteAsync(int id)
    {
        var metodo = await _context.MetodiPagamento.FindAsync(id);
        if (metodo != null)
        {
            _context.MetodiPagamento.Remove(metodo);
            await _context.SaveChangesAsync();
        }
    }
}
