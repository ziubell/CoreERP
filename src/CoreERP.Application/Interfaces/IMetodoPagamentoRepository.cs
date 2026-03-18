using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface IMetodoPagamentoRepository
{
    Task<List<MetodoPagamento>> GetAllAsync(bool? attivo = null);
    Task<MetodoPagamento?> GetByIdAsync(int id);
    Task<MetodoPagamento> AddAsync(MetodoPagamento metodo);
    Task UpdateAsync(MetodoPagamento metodo);
    Task<bool> IsInUseAsync(int id);
    Task DeleteAsync(int id);
}
