using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Attivita;

public class Attivita : AggregateRoot
{
    public TipoAttivita Tipo { get; set; }
    public StatoAttivita Stato { get; set; } = StatoAttivita.Aperta;
    public PrioritaAttivita Priorita { get; set; } = PrioritaAttivita.Media;
    public string? Titolo { get; set; }
    public string? Descrizione { get; set; }
    public string? Colore { get; set; }
    public string? Tag { get; set; }

    // Date
    public DateTime DataRichiesta { get; set; }
    public DateTime? DataInizio { get; set; }
    public DateTime? DataFine { get; set; }
    public DateTime? DataScadenza { get; set; }

    // Chiusura
    public string? ChiusuraMotivo { get; set; }
    public string? ChiusuraUserId { get; set; }
    public DateTime? ChiusuraData { get; set; }

    // Indirizzo intervento
    public string? Indirizzo { get; set; }
    public string? Cap { get; set; }
    public string? Provincia { get; set; }
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }

    // CPE & Pricing
    public string? CPE { get; set; }
    public decimal Prezzo { get; set; }

    // Relations
    public int? AnagraficaId { get; set; }
    public Anagrafica.Anagrafica? Anagrafica { get; set; }

    public int? ImpiantoId { get; set; }

    public int? AttivitaPadreId { get; set; }
    public Attivita? AttivitaPadre { get; set; }

    // Navigation
    public ICollection<AttivitaUser> Users { get; set; } = [];
    public ICollection<AttivitaToDo> ToDo { get; set; } = [];
    public ICollection<AttivitaPianificazione> Pianificazioni { get; set; } = [];
    public AttivitaFatturazione? Fatturazione { get; set; }
}
