using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Notifications;

public class TipoNotifica : BaseEntity
{
    public string Codice { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty;
    public string? Icona { get; set; }
    public string? Colore { get; set; }
    public bool Attivo { get; set; } = true;
}
