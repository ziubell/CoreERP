using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface ITipoTecnologiaRepository
{
    Task<List<TipoTecnologia>> GetAllAsync(bool? attivo = null);
    Task<TipoTecnologia?> GetByIdAsync(int id);
    Task<TipoTecnologia> AddAsync(TipoTecnologia entity);
    Task UpdateAsync(TipoTecnologia entity);
    Task DeleteAsync(int id);
    Task<bool> IsInUseAsync(int id);
}
