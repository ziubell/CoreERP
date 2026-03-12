using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Commerciale;

public class Contratto : AggregateRoot
{
    public StatoContratto Stato { get; set; } = StatoContratto.Attivo;
    public string? Tipo { get; set; }
    public string? Numero { get; set; }
    public DateTime DataInizio { get; set; }
    public DateTime? DataFine { get; set; }

    // Relations
    public int AnagraficaId { get; set; }
    public Anagrafica.Anagrafica Anagrafica { get; set; } = null!;
    public int? ImpiantoId { get; set; }

    // Navigation
    public ICollection<ContrattoRiga> Righe { get; set; } = [];
    public ICollection<ContrattoMeta> Meta { get; set; } = [];
}

public class ContrattoRiga : BaseEntity
{
    public int ContrattoId { get; set; }
    public Contratto Contratto { get; set; } = null!;
    public string Descrizione { get; set; } = string.Empty;
    public int Quantita { get; set; } = 1;
    public decimal PrezzoUnitario { get; set; }
}

public class ContrattoMeta : BaseEntity
{
    public int ContrattoId { get; set; }
    public Contratto Contratto { get; set; } = null!;
    public string Chiave { get; set; } = string.Empty;
    public string? Valore { get; set; }
}
