using CoreERP.Domain.Entities.Messaggi;

namespace CoreERP.Application.Interfaces;

public interface IMessaggioRepository
{
    Task<List<Messaggio>> GetByEntitaAsync(string entitaTipo, int entitaId, int pagina = 1, int dimensionePagina = 20);
    Task<int> CountByEntitaAsync(string entitaTipo, int entitaId);
    Task<Messaggio?> GetByIdAsync(int id);
    Task<AllegatoMessaggio?> GetAllegatoByIdAsync(int allegatoId);
    Task<Messaggio> AddAsync(Messaggio messaggio);
    Task UpdateAsync(Messaggio messaggio);
    Task DeleteAsync(int id);
}
