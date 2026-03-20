using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Notifications;

public class SottoscrizioneNotifica : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string EntitaTipo { get; set; } = string.Empty;
    public int EntitaId { get; set; }
    public DateTime DataSottoscrizione { get; set; } = DateTime.UtcNow;

    // Preferenze notifica per questa entità
    public bool NotificaModifiche { get; set; } = true;
    public bool NotificaMessaggi { get; set; } = true;
    public bool NotificaContatti { get; set; } = true;
    public bool NotificaIndirizzi { get; set; } = true;
}
