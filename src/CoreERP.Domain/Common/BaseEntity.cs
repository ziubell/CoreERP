namespace CoreERP.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime DataCreazione { get; set; }
    public string? CreatoDA { get; set; }
    public DateTime? DataModifica { get; set; }
    public string? ModificatoDa { get; set; }
}

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DataCancellazione { get; set; }
    public string? CancellatoDa { get; set; }
}
