using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using CoreERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class AnagraficaRepository : IAnagraficaRepository
{
    private readonly ApplicationDbContext _context;

    public AnagraficaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Anagrafica>> GetListAsync(TipoAnagrafica? tipo = null, bool? attivo = null,
        string? ricerca = null, int pagina = 1, int dimensionePagina = 20)
    {
        var query = BuildQuery(tipo, attivo, ricerca);

        return await query
            .OrderByDescending(a => a.DataCreazione)
            .Skip((pagina - 1) * dimensionePagina)
            .Take(dimensionePagina)
            .ToListAsync();
    }

    public async Task<int> CountAsync(TipoAnagrafica? tipo = null, bool? attivo = null, string? ricerca = null)
    {
        return await BuildQuery(tipo, attivo, ricerca).CountAsync();
    }

    public async Task<Anagrafica?> GetByIdAsync(int id, bool includeContatti = false)
    {
        var query = _context.Anagrafiche
            .Include(a => a.MotivoDisattivazione)
            .Include(a => a.MetodoPagamento)
            .AsQueryable();

        if (includeContatti)
        {
            query = query.Include(a => a.AnagraficaContatti)
                .ThenInclude(ac => ac.Contatto)
                .Include(a => a.AnagraficaContatti)
                .ThenInclude(ac => ac.RuoloContatto);
        }

        return await query.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Anagrafica> AddAsync(Anagrafica anagrafica)
    {
        _context.Anagrafiche.Add(anagrafica);
        await _context.SaveChangesAsync();
        return anagrafica;
    }

    public async Task UpdateAsync(Anagrafica anagrafica)
    {
        _context.Anagrafiche.Update(anagrafica);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsPartitaIvaAsync(string partitaIva, int? excludeId = null)
    {
        var query = _context.Anagrafiche.Where(a => a.PartitaIva == partitaIva);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsCodiceFiscaleAsync(string codiceFiscale, int? excludeId = null)
    {
        var query = _context.Anagrafiche.Where(a => a.CodiceFiscale == codiceFiscale);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<(int Id, string Denominazione)?> GetByPartitaIvaAsync(string partitaIva, int? excludeId = null)
    {
        var query = _context.Anagrafiche.Where(a => a.PartitaIva == partitaIva);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        var result = await query.Select(a => new { a.Id, a.Denominazione }).FirstOrDefaultAsync();
        return result != null ? (result.Id, result.Denominazione) : null;
    }

    public async Task<(int Id, string Denominazione)?> GetByCodiceFiscaleAsync(string codiceFiscale, int? excludeId = null)
    {
        var query = _context.Anagrafiche.Where(a => a.CodiceFiscale == codiceFiscale);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        var result = await query.Select(a => new { a.Id, a.Denominazione }).FirstOrDefaultAsync();
        return result != null ? (result.Id, result.Denominazione) : null;
    }

    public async Task<int> GetNextCodiceClienteAsync()
    {
        var max = await _context.Anagrafiche
            .IgnoreQueryFilters()
            .Where(a => a.CodiceCliente != null)
            .MaxAsync(a => (int?)a.CodiceCliente) ?? 0;
        return max + 1;
    }

    private IQueryable<Anagrafica> BuildQuery(TipoAnagrafica? tipo, bool? attivo, string? ricerca)
    {
        var query = _context.Anagrafiche.AsQueryable();

        if (tipo.HasValue)
            query = query.Where(a => a.Tipo == tipo.Value);

        if (attivo.HasValue)
            query = query.Where(a => a.Attivo == attivo.Value);

        if (!string.IsNullOrWhiteSpace(ricerca))
        {
            var term = ricerca.Trim();
            query = query.Where(a =>
                a.Denominazione.Contains(term) ||
                (a.PartitaIva != null && a.PartitaIva.Contains(term)) ||
                (a.CodiceFiscale != null && a.CodiceFiscale.Contains(term)) ||
                (a.PEC != null && a.PEC.Contains(term)));
        }

        return query;
    }
}
