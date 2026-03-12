using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Comunicazioni;

public class Comunicazione : AuditableEntity
{
    public TipoComunicazione Tipo { get; set; }
    public StatoComunicazione Stato { get; set; } = StatoComunicazione.Bozza;
    public string? Titolo { get; set; }
    public string? Testo { get; set; }
    public string? EmailFrom { get; set; }
    public string? Email { get; set; }
    public string? Cellulare { get; set; }

    // Tracking Brevo
    public string? ExternalId { get; set; }
    public string? ExternalStatus { get; set; }
    public string? ExternalStatusDetail { get; set; }
    public string? Piattaforma { get; set; }

    // Relations
    public string? UserId { get; set; }
    public int? AnagraficaId { get; set; }
    public Anagrafica.Anagrafica? Anagrafica { get; set; }
    public int? ComunicazioneMassivaId { get; set; }
}

public class Messaggio : AuditableEntity
{
    public string? EntitaId { get; set; }
    public string? EntitaTipo { get; set; }
    public string? Testo { get; set; }
    public PrioritaMessaggio Priorita { get; set; } = PrioritaMessaggio.Media;
    public AreaMessaggio Area { get; set; } = AreaMessaggio.Generale;
    public string? Tag { get; set; }

    public string? UserId { get; set; }

    public ICollection<MessaggioUserStatus> UsersStatus { get; set; } = [];
}

public class MessaggioUserStatus : BaseEntity
{
    public int MessaggioId { get; set; }
    public Messaggio Messaggio { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public bool Letto { get; set; }
    public DateTime? DataLettura { get; set; }
}
