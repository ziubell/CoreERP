using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class StoricoModificaRepository : IStoricoModificaRepository
{
    private readonly ApplicationDbContext _context;

    public StoricoModificaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StoricoModifica>> GetByEntitaAsync(string entitaTipo, int entitaId,
        int pagina = 1, int dimensionePagina = 50)
    {
        return await _context.StoricoModifiche
            .Where(s => s.EntitaTipo == entitaTipo && s.EntitaId == entitaId)
            .OrderByDescending(s => s.DataModifica)
            .Skip((pagina - 1) * dimensionePagina)
            .Take(dimensionePagina)
            .ToListAsync();
    }

    public async Task<StoricoModifica?> GetByIdAsync(int id)
        => await _context.StoricoModifiche.FindAsync(id);

    public async Task AddAsync(StoricoModifica storico)
    {
        _context.StoricoModifiche.Add(storico);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<StoricoModifica> storici)
    {
        _context.StoricoModifiche.AddRange(storici);
        await _context.SaveChangesAsync();
    }
}
