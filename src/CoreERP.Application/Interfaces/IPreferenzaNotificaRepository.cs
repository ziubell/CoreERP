using CoreERP.Application.DTOs;
using CoreERP.Domain.Entities.Notifications;

namespace CoreERP.Application.Interfaces;

public interface IPreferenzaNotificaRepository
{
    Task<List<PreferenzaNotificaUtente>> GetByUserAsync(string userId);
    Task<PreferenzaNotificaUtente?> GetByUserAndTipoAsync(string userId, int tipoNotificaId);
    Task SalvaPreferenzeAsync(string userId, List<PreferenzaNotificaDto> preferenze);
}
