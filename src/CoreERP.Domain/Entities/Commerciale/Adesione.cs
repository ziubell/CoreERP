using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Commerciale;

public class Adesione : AggregateRoot
{
    public StatoAdesione Stato { get; set; } = StatoAdesione.Bozza;
    public string? Tipo { get; set; }
    public string? Voucher { get; set; }

    // Dati cliente
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Email { get; set; }
    public string? Cellulare { get; set; }
    public string? CodiceFiscale { get; set; }

    // Impianto / Installazione
    public string? ImpiantoIndirizzo { get; set; }
    public string? ImpiantoCap { get; set; }
    public string? ImpiantoProvincia { get; set; }
    public TipoCopertura? ImpiantoTipo { get; set; }
    public double? ImpiantoLatitudine { get; set; }
    public double? ImpiantoLongitudine { get; set; }
    public string? ImpiantoIdBuilding { get; set; }
    public string? ImpiantoPCN { get; set; }

    // Tracciamento email
    public bool EmailInviata { get; set; }
    public bool EmailFirmataInviata { get; set; }
    public DateTime? DataFirmata { get; set; }
    public bool IsCompleta { get; set; }

    // Relations
    public int? AnagraficaId { get; set; }
    public Anagrafica.Anagrafica? Anagrafica { get; set; }
    public string? UserId { get; set; }
    public int? DocumentoId { get; set; }

    // Navigation
    public ICollection<AdesioneArticolo> Articoli { get; set; } = [];
    public ICollection<AdesionePromozione> Promozioni { get; set; } = [];
}
