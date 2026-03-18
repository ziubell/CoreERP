using CoreERP.Domain.Entities.Anagrafica;

namespace CoreERP.Application.Interfaces;

public interface IStoricoModificaRepository
{
    Task<List<StoricoModifica>> GetByEntitaAsync(string entitaTipo, int entitaId, int pagina = 1, int dimensionePagina = 50);
    Task<StoricoModifica?> GetByIdAsync(int id);
    Task AddAsync(StoricoModifica storico);
    Task AddRangeAsync(IEnumerable<StoricoModifica> storici);
}
