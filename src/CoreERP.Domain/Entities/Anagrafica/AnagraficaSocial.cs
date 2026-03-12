using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class AnagraficaSocial : BaseEntity
{
    public int AnagraficaId { get; set; }
    public Anagrafica Anagrafica { get; set; } = null!;
    public string Piattaforma { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Username { get; set; }
}
