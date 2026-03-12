using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Anagrafica;

public class Anagrafica : AggregateRoot
{
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? RagioneSociale { get; set; }
    public bool IsPersonaGiuridica { get; set; }
    public TipoAnagrafica Tipo { get; set; }
    public StatoAnagrafica Stato { get; set; } = StatoAnagrafica.Attivo;

    // Dati fiscali
    public string? CodiceFiscale { get; set; }
    public string? PartitaIva { get; set; }
    public string? CodiceSDI { get; set; }
    public string? PEC { get; set; }

    // Indirizzo
    public string? Indirizzo { get; set; }
    public string? Civico { get; set; }
    public string? Localita { get; set; }
    public string? Frazione { get; set; }
    public string? Cap { get; set; }
    public string? Provincia { get; set; }
    public string Stato_Indirizzo { get; set; } = "Italia";
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }

    // Geocoding EGON
    public string? EgonCivico { get; set; }
    public string? EgonComune { get; set; }
    public string? EgonStrada { get; set; }

    // Pagamento
    public MetodoPagamento? PagamentoMetodo { get; set; }
    public string? PagamentoIBAN { get; set; }
    public PeriodicitaPagamento? PagamentoPeriodicita { get; set; }

    // Brevo sync
    public bool BrevoNeedsSync { get; set; }
    public DateTime? BrevoLastSyncDate { get; set; }

    // Self-referencing (anagrafica padre)
    public int? PadreId { get; set; }
    public Anagrafica? Padre { get; set; }

    // Navigation properties
    public ICollection<AnagraficaMeta> Meta { get; set; } = [];
    public ICollection<AnagraficaContatto> Contatti { get; set; } = [];
    public ICollection<AnagraficaUser> Users { get; set; } = [];
    public ICollection<AnagraficaSocial> Social { get; set; } = [];

    // Computed
    public string NomeCompleto => IsPersonaGiuridica
        ? RagioneSociale ?? string.Empty
        : $"{Nome} {Cognome}".Trim();
}
