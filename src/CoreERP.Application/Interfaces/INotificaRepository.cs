using CoreERP.Domain.Entities.Notifications;

namespace CoreERP.Application.Interfaces;

public interface INotificaRepository
{
    Task<List<Notifica>> GetByUserAsync(string userId, bool soloNonLette = false,
        int pagina = 1, int dimensionePagina = 20);
    Task<int> ContaNonLetteAsync(string userId);
    Task SegnaComeLettaAsync(int notificaId, string userId);
    Task SegnaTutteComeLettaAsync(string userId);
    Task EliminaAsync(int notificaId, string userId);
    Task<Notifica> AddAsync(Notifica notifica);
}
