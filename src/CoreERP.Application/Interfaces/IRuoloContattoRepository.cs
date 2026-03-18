using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface IRuoloContattoRepository
{
    Task<List<RuoloContatto>> GetAllAsync(bool? attivo = null);
    Task<RuoloContatto?> GetByIdAsync(int id);
    Task<RuoloContatto> AddAsync(RuoloContatto ruolo);
    Task UpdateAsync(RuoloContatto ruolo);
    Task<bool> IsInUseAsync(int id);
    Task DeleteAsync(int id);
}
