using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Rete;

public class ApparatoDiRete : AuditableEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public TipoApparato Tipo { get; set; }
    public StatoApparato Stato { get; set; } = StatoApparato.Attivo;
    public DateTime? DataInstallazione { get; set; }
    public string? Colore { get; set; }

    // Posizione geografica (latitudine, longitudine per punti; GeoJSON per linee/poligoni)
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }
    public string? GeoJson { get; set; }
}

public class Armadio : ApparatoDiRete
{
    public ICollection<ApparatoDiRete> SwitchesContenuti { get; set; } = [];
    public ICollection<ApparatoDiRete> SplitterContenuti { get; set; } = [];
}

public class Cavo : ApparatoDiRete
{
    public ICollection<Fibra> Fibre { get; set; } = [];
}

public class Fibra : ApparatoDiRete
{
    public int CavoId { get; set; }
    public Cavo Cavo { get; set; } = null!;
    public string? StatoFibra { get; set; }
    public string? ColoreIdentificativo { get; set; }
}

public class Splitter : ApparatoDiRete
{
    public string? TipoSplitter { get; set; }
    public double? AttenuazioneDb { get; set; }
    public int? InputFibraId { get; set; }
}

public class Giunto : ApparatoDiRete
{
    public string? TipoGiunto { get; set; }
    public double? AttenuazioneDb { get; set; }
}
