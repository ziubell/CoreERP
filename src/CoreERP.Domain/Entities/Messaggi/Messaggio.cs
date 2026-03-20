using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Messaggi;

public class Messaggio : AuditableEntity
{
    public string EntitaTipo { get; set; } = string.Empty; // "Anagrafica", "Contatto", etc.
    public int EntitaId { get; set; }
    public string Testo { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    public ICollection<AllegatoMessaggio> Allegati { get; set; } = new List<AllegatoMessaggio>();
}
