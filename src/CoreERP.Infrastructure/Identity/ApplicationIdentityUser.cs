using Microsoft.AspNetCore.Identity;

namespace CoreERP.Infrastructure.Identity;

public class ApplicationIdentityUser : IdentityUser
{
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Cellulare { get; set; }
    public string? Ruolo { get; set; }
    public string? Foto { get; set; }
    public bool Dipendente { get; set; }
    public bool Reperibile { get; set; }
    public string? CodiceAgente { get; set; }
    public DateTime? DataLogin { get; set; }
    public DateTime? DataUltimoLogin { get; set; }
    public string? Token { get; set; }

    public string NomeCompleto => $"{Nome} {Cognome}".Trim();
}
