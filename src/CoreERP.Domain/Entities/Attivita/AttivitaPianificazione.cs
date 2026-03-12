using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Attivita;

public class AttivitaPianificazione : AuditableEntity
{
    public int AttivitaId { get; set; }
    public Attivita Attivita { get; set; } = null!;
    public int SquadraId { get; set; }
    public DateTime DataPianificazione { get; set; }
    public string? Note { get; set; }
}
