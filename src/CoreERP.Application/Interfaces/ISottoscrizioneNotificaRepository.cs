using CoreERP.Domain.Entities.Notifications;

namespace CoreERP.Application.Interfaces;

public interface ISottoscrizioneNotificaRepository
{
    Task<bool> IsFollowingAsync(string userId, string entitaTipo, int entitaId);
    Task FollowAsync(string userId, string entitaTipo, int entitaId);
    Task UnfollowAsync(string userId, string entitaTipo, int entitaId);
    Task<List<string>> GetFollowersAsync(string entitaTipo, int entitaId);
    Task<List<SottoscrizioneNotifica>> GetByUserAsync(string userId, string? entitaTipo = null);
    Task<SottoscrizioneNotifica?> GetAsync(string userId, string entitaTipo, int entitaId);
    Task UpdateAsync(SottoscrizioneNotifica sottoscrizione);
}
