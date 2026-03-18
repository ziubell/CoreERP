using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class RuoloContatto : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public bool Attivo { get; set; } = true;
    public int Ordine { get; set; }
}
