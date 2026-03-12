using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Commerciale;

public class Preventivo : AggregateRoot
{
    public StatoPreventivo Stato { get; set; } = StatoPreventivo.Bozza;
    public string? Numero { get; set; }
    public int Versione { get; set; } = 1;
    public DateTime Data { get; set; }
    public string? Tag { get; set; }
    public bool EmailInviata { get; set; }

    // Relations
    public int AnagraficaId { get; set; }
    public Anagrafica.Anagrafica Anagrafica { get; set; } = null!;
    public string? UserId { get; set; }
    public int? DocumentoId { get; set; }

    // Navigation
    public ICollection<PreventivoSezione> Sezioni { get; set; } = [];
}
