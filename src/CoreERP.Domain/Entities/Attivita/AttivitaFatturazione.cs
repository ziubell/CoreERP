using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Attivita;

public class AttivitaFatturazione : AuditableEntity
{
    public int AttivitaId { get; set; }
    public Attivita Attivita { get; set; } = null!;
    public bool Fatturata { get; set; }
    public DateTime? DataFatturazione { get; set; }
    public string? NumeroFattura { get; set; }
    public decimal Importo { get; set; }
    public string? Note { get; set; }
}
