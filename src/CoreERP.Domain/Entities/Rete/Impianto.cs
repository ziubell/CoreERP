using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Rete;

public class Impianto : AggregateRoot
{
    public string? Nome { get; set; }
    public TipoImpianto Tipo { get; set; }
    public StatoImpianto Stato { get; set; } = StatoImpianto.Attivo;
    public string? Profilo { get; set; }

    // Indirizzo
    public string? Indirizzo { get; set; }
    public string? Cap { get; set; }
    public string? Provincia { get; set; }
    public string Stato_Indirizzo { get; set; } = "Italia";
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }

    // Copertura
    public TipoCopertura? CoperturaDisponibile { get; set; }
    public DateTime? CoperturaData { get; set; }

    // OpenFiber
    public string? OpenFiberPCN { get; set; }
    public string? OpenFiberIDBuilding { get; set; }
    public string? IDRisorsa { get; set; }
    public string? CodiceMigrazione { get; set; }

    // Rete
    public bool CollegatoHub { get; set; }
    public bool NecessariaPiattaforma { get; set; }
    public string? PosizioneRouter { get; set; }
    public string? PosizionePOE { get; set; }

    // Gerarchia
    public int? ImpiantoPadreId { get; set; }
    public Impianto? ImpiantoPadre { get; set; }
    public ICollection<Impianto> ImpiantiFigli { get; set; } = [];

    // Navigation
    public int? GiuntoId { get; set; }
}
