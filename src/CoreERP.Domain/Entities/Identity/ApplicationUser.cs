namespace CoreERP.Domain.Entities.Identity;

/// <summary>
/// Extends IdentityUser with custom fields.
/// The actual inheritance from IdentityUser happens in the Infrastructure layer.
/// Here we define only the domain-specific properties.
/// </summary>
public class ApplicationUser
{
    public string Id { get; set; } = string.Empty;
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Email { get; set; }
    public string? Cellulare { get; set; }
    public string? Ruolo { get; set; }
    public string? Foto { get; set; }
    public bool Dipendente { get; set; }
    public bool Reperibile { get; set; }
    public string? CodiceAgente { get; set; }
    public DateTime? DataLogin { get; set; }
    public DateTime? DataUltimoLogin { get; set; }
    public string? Token { get; set; }

    // Microsoft 365 integration
    public string? MicrosoftId { get; set; }
    public string? MicrosoftEmail { get; set; }
    public DateTime? DataCollegamentoMicrosoft { get; set; }

    public string NomeCompleto => $"{Nome} {Cognome}".Trim();
}
