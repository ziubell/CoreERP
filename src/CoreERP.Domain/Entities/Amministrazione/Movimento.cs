using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Amministrazione;

public class Movimento : AuditableEntity
{
    public DateTime DataMovimento { get; set; }
    public string? Causale { get; set; }
    public string? Descrizione { get; set; }
    public decimal Importo { get; set; }
    public string? Stato { get; set; }
    public string? Tipo { get; set; }

    public int AnagraficaId { get; set; }
    public Anagrafica.Anagrafica Anagrafica { get; set; } = null!;
}

public class Scadenziario : AuditableEntity
{
    public string? Descrizione { get; set; }
    public DateTime DataInizio { get; set; }
    public DateTime DataScadenza { get; set; }
    public string? Ricorrenza { get; set; }

    public int AnagraficaId { get; set; }
    public Anagrafica.Anagrafica Anagrafica { get; set; } = null!;

    public ICollection<ScadenziarioLog> Logs { get; set; } = [];
}

public class ScadenziarioLog : BaseEntity
{
    public int ScadenziarioId { get; set; }
    public Scadenziario Scadenziario { get; set; } = null!;
    public DateTime Data { get; set; }
    public string? Note { get; set; }
}

public class Bank : AuditableEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? InstitutionId { get; set; }
    public decimal Saldo { get; set; }
    public decimal? Fido { get; set; }
    public DateTime? DataSaldo { get; set; }
}
