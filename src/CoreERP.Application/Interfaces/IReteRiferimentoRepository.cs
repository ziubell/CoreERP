using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface IReteRiferimentoRepository
{
    Task<List<ReteRiferimento>> GetAllAsync(bool? attivo = null);
    Task<ReteRiferimento?> GetByIdAsync(int id);
    Task<ReteRiferimento> AddAsync(ReteRiferimento entity);
    Task UpdateAsync(ReteRiferimento entity);
    Task DeleteAsync(int id);
    Task<bool> IsInUseAsync(int id);
}
