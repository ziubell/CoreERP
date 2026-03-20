using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface IIndirizzoRepository
{
    Task<List<Indirizzo>> GetByAnagraficaIdAsync(int anagraficaId);
    Task<List<Indirizzo>> GetAllAsync(string? tipo = null, string? ricerca = null, int pagina = 1, int dimensionePagina = 20);
    Task<int> GetCountAsync(string? tipo = null, string? ricerca = null);
    Task<Indirizzo?> GetByIdAsync(int id);
    Task<Indirizzo> AddAsync(Indirizzo indirizzo);
    Task UpdateAsync(Indirizzo indirizzo);
    Task DeleteAsync(int id);
}
