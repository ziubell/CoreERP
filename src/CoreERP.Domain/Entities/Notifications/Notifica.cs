using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Notifications;

public class Notifica : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? MittenteUserId { get; set; }
    public int TipoNotificaId { get; set; }
    public string Titolo { get; set; } = string.Empty;
    public string? Messaggio { get; set; }
    public string? Link { get; set; }
    public bool Letta { get; set; }
    public DateTime? DataLettura { get; set; }

    public TipoNotifica TipoNotifica { get; set; } = null!;
}
