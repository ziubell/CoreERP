using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface IContattoRepository
{
    Task<List<Contatto>> GetListAsync(string? ricerca = null, int pagina = 1, int dimensionePagina = 20);
    Task<int> CountAsync(string? ricerca = null);
    Task<Contatto?> GetByIdAsync(int id, bool includeAnagrafiche = false);
    Task<Contatto> AddAsync(Contatto contatto);
    Task UpdateAsync(Contatto contatto);
    Task<bool> ExistsEmailAsync(string email, int? excludeId = null);
    Task<bool> ExistsCellulareAsync(string cellulare, int? excludeId = null);
}
