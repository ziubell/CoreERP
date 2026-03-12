using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Commerciale;

public class AdesioneArticolo : BaseEntity
{
    public int AdesioneId { get; set; }
    public Adesione Adesione { get; set; } = null!;
    public int ArticoloId { get; set; }
    public int Quantita { get; set; } = 1;
    public decimal Prezzo { get; set; }
    public string? Note { get; set; }
}
