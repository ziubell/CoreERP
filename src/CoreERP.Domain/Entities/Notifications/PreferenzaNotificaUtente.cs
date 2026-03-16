using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Notifications;

public class PreferenzaNotificaUtente : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int TipoNotificaId { get; set; }
    public bool Email { get; set; } = true;
    public bool Browser { get; set; } = true;
    public bool Teams { get; set; }

    public TipoNotifica TipoNotifica { get; set; } = null!;
}
