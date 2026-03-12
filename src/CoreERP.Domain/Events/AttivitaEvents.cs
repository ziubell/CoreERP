using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Events;

public sealed record AttivitaCreatedEvent(int AttivitaId) : DomainEvent;
public sealed record AttivitaStatusChangedEvent(int AttivitaId, StatoAttivita OldStato, StatoAttivita NewStato) : DomainEvent;
public sealed record AttivitaAssignedEvent(int AttivitaId, string UserId) : DomainEvent;
public sealed record AttivitaClosedEvent(int AttivitaId, string? Motivo) : DomainEvent;
