using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Notifications;

public class ImpostazioniNotificaUtente : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int GiorniRetention { get; set; } = 90;
}
