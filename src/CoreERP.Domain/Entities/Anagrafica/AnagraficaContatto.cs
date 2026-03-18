using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class AnagraficaContatto : AuditableEntity
{
    public int AnagraficaId { get; set; }
    public int ContattoId { get; set; }
    public int RuoloContattoId { get; set; }
    public bool Principale { get; set; }

    // Navigation
    public Anagrafica Anagrafica { get; set; } = null!;
    public Contatto Contatto { get; set; } = null!;
    public RuoloContatto RuoloContatto { get; set; } = null!;
}
