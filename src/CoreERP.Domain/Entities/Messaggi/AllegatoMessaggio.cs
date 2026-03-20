using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Messaggi;

public class AllegatoMessaggio : BaseEntity
{
    public int MessaggioId { get; set; }
    public string NomeFile { get; set; } = string.Empty;
    public string Percorso { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Dimensione { get; set; }
    public DateTime DataCaricamento { get; set; }

    public Messaggio Messaggio { get; set; } = null!;
}
