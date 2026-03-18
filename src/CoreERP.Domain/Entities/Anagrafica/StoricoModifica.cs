using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class StoricoModifica : BaseEntity
{
    public string EntitaTipo { get; set; } = string.Empty;
    public int EntitaId { get; set; }
    public string Campo { get; set; } = string.Empty;
    public string? ValorePrecedente { get; set; }
    public string? ValoreNuovo { get; set; }
    public string? ValorePrecedenteLabel { get; set; }
    public string? ValoreNuovoLabel { get; set; }
    public DateTime DataModifica { get; set; }
    public string ModificatoDa { get; set; } = string.Empty;
    public string? Note { get; set; }
}
