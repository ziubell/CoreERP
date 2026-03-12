using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class AnagraficaMeta : BaseEntity
{
    public int AnagraficaId { get; set; }
    public Anagrafica Anagrafica { get; set; } = null!;
    public string Chiave { get; set; } = string.Empty;
    public string? Valore { get; set; }
}
