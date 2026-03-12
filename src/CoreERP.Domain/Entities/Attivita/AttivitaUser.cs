using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Attivita;

public class AttivitaUser : BaseEntity
{
    public int AttivitaId { get; set; }
    public Attivita Attivita { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
}
