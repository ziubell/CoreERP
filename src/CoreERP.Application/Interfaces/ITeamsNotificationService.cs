namespace CoreERP.Application.Interfaces;

public interface ITeamsNotificationService
{
    Task InviaAsync(string userId, string titolo, string? messaggio = null, string? link = null);
}
