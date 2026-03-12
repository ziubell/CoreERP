using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Commerciale;

public class PreventivoSezione : BaseEntity
{
    public int PreventivoId { get; set; }
    public Preventivo Preventivo { get; set; } = null!;
    public string Titolo { get; set; } = string.Empty;
    public int Ordine { get; set; }

    public ICollection<PreventivoVoce> Voci { get; set; } = [];
}

public class PreventivoVoce : BaseEntity
{
    public int PreventivoSezioneId { get; set; }
    public PreventivoSezione Sezione { get; set; } = null!;
    public string Descrizione { get; set; } = string.Empty;
    public int Quantita { get; set; } = 1;
    public decimal PrezzoUnitario { get; set; }
    public int Ordine { get; set; }
}
