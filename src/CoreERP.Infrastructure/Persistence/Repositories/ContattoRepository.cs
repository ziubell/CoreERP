using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class ContattoRepository : IContattoRepository
{
    private readonly ApplicationDbContext _context;

    public ContattoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Contatto>> GetListAsync(string? ricerca = null, int pagina = 1, int dimensionePagina = 20)
    {
        var query = _context.Contatti.AsQueryable();

        if (!string.IsNullOrWhiteSpace(ricerca))
        {
            var term = ricerca.Trim();
            query = query.Where(c =>
                c.Nome.Contains(term) ||
                c.Cognome.Contains(term) ||
                (c.Email != null && c.Email.Contains(term)) ||
                (c.Cellulare != null && c.Cellulare.Contains(term)));
        }

        return await query
            .OrderByDescending(c => c.DataCreazione)
            .Skip((pagina - 1) * dimensionePagina)
            .Take(dimensionePagina)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? ricerca = null)
    {
        var query = _context.Contatti.AsQueryable();

        if (!string.IsNullOrWhiteSpace(ricerca))
        {
            var term = ricerca.Trim();
            query = query.Where(c =>
                c.Nome.Contains(term) ||
                c.Cognome.Contains(term) ||
                (c.Email != null && c.Email.Contains(term)) ||
                (c.Cellulare != null && c.Cellulare.Contains(term)));
        }

        return await query.CountAsync();
    }

    public async Task<Contatto?> GetByIdAsync(int id, bool includeAnagrafiche = false)
    {
        var query = _context.Contatti.AsQueryable();

        if (includeAnagrafiche)
        {
            query = query.Include(c => c.AnagraficaContatti)
                .ThenInclude(ac => ac.Anagrafica)
                .Include(c => c.AnagraficaContatti)
                .ThenInclude(ac => ac.RuoloContatto);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contatto> AddAsync(Contatto contatto)
    {
        _context.Contatti.Add(contatto);
        await _context.SaveChangesAsync();
        return contatto;
    }

    public async Task UpdateAsync(Contatto contatto)
    {
        _context.Contatti.Update(contatto);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
    {
        var query = _context.Contatti.Where(c => c.Email == email);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsCellulareAsync(string cellulare, int? excludeId = null)
    {
        var query = _context.Contatti.Where(c => c.Cellulare == cellulare);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
