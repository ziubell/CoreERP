using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Commerciale;

public class Documento : AggregateRoot
{
    public TipoDocumento Tipo { get; set; }
    public StatoDocumento Stato { get; set; } = StatoDocumento.Bozza;
    public string? Numero { get; set; }
    public DateTime Data { get; set; }
    public string? Causale { get; set; }
    public string? Trasporto { get; set; }
    public string? Vettore { get; set; }

    // Relations
    public int AnagraficaId { get; set; }
    public Anagrafica.Anagrafica Anagrafica { get; set; } = null!;

    // Navigation
    public ICollection<DocumentoRiga> Righe { get; set; } = [];
}

public class DocumentoRiga : BaseEntity
{
    public int DocumentoId { get; set; }
    public Documento Documento { get; set; } = null!;
    public string Descrizione { get; set; } = string.Empty;
    public int Quantita { get; set; } = 1;
    public decimal PrezzoUnitario { get; set; }
    public decimal? Sconto { get; set; }
    public int Ordine { get; set; }
}
