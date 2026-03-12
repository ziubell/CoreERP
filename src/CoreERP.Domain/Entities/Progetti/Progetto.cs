using CoreERP.Domain.Common;

namespace CoreERP.Domain.Entities.Progetti;

public class Progetto : AggregateRoot
{
    public string Nome { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public string? Stato { get; set; }
    public string? UserId { get; set; }

    public ICollection<ProgettoToDo> ToDo { get; set; } = [];
}

public class ProgettoToDo : AuditableEntity
{
    public int ProgettoId { get; set; }
    public Progetto Progetto { get; set; } = null!;
    public string Descrizione { get; set; } = string.Empty;
    public bool Completato { get; set; }
    public DateTime? DataCompletamento { get; set; }
}
