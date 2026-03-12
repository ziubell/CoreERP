using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Commerciale;

public class AdesionePromozione : BaseEntity
{
    public int AdesioneId { get; set; }
    public Adesione Adesione { get; set; } = null!;
    public int PromozioneId { get; set; }
}
