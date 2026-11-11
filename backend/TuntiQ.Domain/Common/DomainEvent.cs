namespace TuntiQ.Domain.Common;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid? UserId { get; init; }
    public Guid? BatchId { get; init; }
}