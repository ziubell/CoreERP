using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Attivita;

public class AttivitaToDo : AuditableEntity
{
    public int AttivitaId { get; set; }
    public Attivita Attivita { get; set; } = null!;
    public string Descrizione { get; set; } = string.Empty;
    public bool Completato { get; set; }
    public DateTime? DataCompletamento { get; set; }
}
