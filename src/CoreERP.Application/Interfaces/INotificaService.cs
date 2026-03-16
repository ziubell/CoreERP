namespace CoreERP.Application.Interfaces;

public interface INotificaService
{
    Task InviaAsync(string userId, string codiceTipoNotifica, string titolo,
        string? messaggio = null, string? link = null, string? mittenteUserId = null);

    Task InviaMultiplaAsync(IEnumerable<string> userIds, string codiceTipoNotifica, string titolo,
        string? messaggio = null, string? link = null, string? mittenteUserId = null);

    Task InviaAFollowersAsync(string entitaTipo, int entitaId, string codiceTipoNotifica,
        string titolo, string? messaggio = null, string? link = null, string? mittenteUserId = null);
}
