using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class MetodoPagamento : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Codice { get; set; } = string.Empty;
    public bool RichiedeIBAN { get; set; }
    public bool Attivo { get; set; } = true;
    public int Ordine { get; set; }
}
