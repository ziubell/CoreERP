using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class AnagraficaContatto : AuditableEntity
{
    public int AnagraficaId { get; set; }
    public Anagrafica Anagrafica { get; set; } = null!;
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Ruolo { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Cellulare { get; set; }
    public string? Note { get; set; }
}
