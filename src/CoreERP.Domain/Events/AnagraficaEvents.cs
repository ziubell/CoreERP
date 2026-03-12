using CoreERP.Domain.Common;

namespace CoreERP.Domain.Events;

public sealed record AnagraficaCreatedEvent(int AnagraficaId) : DomainEvent;
public sealed record AnagraficaUpdatedEvent(int AnagraficaId) : DomainEvent;
public sealed record AnagraficaDeletedEvent(int AnagraficaId) : DomainEvent;
