using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class Contatto : SoftDeletableEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Cellulare { get; set; }
    public string? Telefono { get; set; }
    public string? Note { get; set; }

    // Brevo sync
    public string? BrevoContactId { get; set; }
    public DateTime? BrevoSyncAt { get; set; }

    // Navigation
    public ICollection<AnagraficaContatto> AnagraficaContatti { get; set; } = new List<AnagraficaContatto>();
}
