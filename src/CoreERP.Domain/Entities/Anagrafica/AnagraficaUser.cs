using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Anagrafica;

public class AnagraficaUser : BaseEntity
{
    public int AnagraficaId { get; set; }
    public Anagrafica Anagrafica { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
}
