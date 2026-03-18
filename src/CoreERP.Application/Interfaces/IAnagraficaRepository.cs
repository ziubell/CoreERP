using CoreERP.Domain.Entities.Anagrafica;
using CoreERP.Domain.Enums;

namespace CoreERP.Application.Interfaces;

public interface IAnagraficaRepository
{
    Task<List<Anagrafica>> GetListAsync(TipoAnagrafica? tipo = null, bool? attivo = null,
        string? ricerca = null, int pagina = 1, int dimensionePagina = 20);
    Task<int> CountAsync(TipoAnagrafica? tipo = null, bool? attivo = null, string? ricerca = null);
    Task<Anagrafica?> GetByIdAsync(int id, bool includeContatti = false);
    Task<Anagrafica> AddAsync(Anagrafica anagrafica);
    Task UpdateAsync(Anagrafica anagrafica);
    Task<bool> ExistsPartitaIvaAsync(string partitaIva, int? excludeId = null);
    Task<bool> ExistsCodiceFiscaleAsync(string codiceFiscale, int? excludeId = null);
    Task<(int Id, string Denominazione)?> GetByPartitaIvaAsync(string partitaIva, int? excludeId = null);
    Task<(int Id, string Denominazione)?> GetByCodiceFiscaleAsync(string codiceFiscale, int? excludeId = null);
    Task<int> GetNextCodiceClienteAsync();
}
