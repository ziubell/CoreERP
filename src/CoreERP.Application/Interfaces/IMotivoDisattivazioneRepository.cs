using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface IMotivoDisattivazioneRepository
{
    Task<List<MotivoDisattivazione>> GetAllAsync(bool? attivo = null);
    Task<MotivoDisattivazione?> GetByIdAsync(int id);
    Task<MotivoDisattivazione> AddAsync(MotivoDisattivazione motivo);
    Task UpdateAsync(MotivoDisattivazione motivo);
    Task<bool> IsInUseAsync(int id);
    Task DeleteAsync(int id);
}
