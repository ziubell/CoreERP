using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class Indirizzo : AuditableEntity
{
    public int AnagraficaId { get; set; }

    // Tipo: Fatturazione, Impianto
    public string Tipo { get; set; } = string.Empty;

    // Solo per Impianto: FTTH, FTTC, FWA
    public string? SottoTipo { get; set; }

    // Solo per Impianto: FIBERCOP, OPENFIBER, SPADHAUSEN, EOLO, etc.
    public string? Rete { get; set; }

    // Indirizzo EGON
    public string Strada { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Frazione { get; set; }
    public string Citta { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string? Regione { get; set; }
    public string? CAP { get; set; }

    // Coordinate
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }

    // Codici EGON
    public string? EgonCivico { get; set; }
    public string? EgonStrada { get; set; }
    public string? EgonLocalita { get; set; }

    public bool Principale { get; set; }

    // Navigation
    public Anagrafica Anagrafica { get; set; } = null!;
}
