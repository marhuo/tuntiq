
using TuntiQ.Domain.Common;

namespace TuntiQ.Infrastructure.Persistence.Ef;


public abstract class EventEntityBase
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; }
    public string Data { get; set; } = null!;
    protected void SetFrom(IDomainEvent domainEvent)
    {
        Id = domainEvent.EventId;
        EventType = DomainEventSerializer.GetTypeName(domainEvent);
        OccurredAt = domainEvent.OccurredAt;
        Data = DomainEventSerializer.Serialize(domainEvent);
    }

    public IDomainEvent ToDomain() => DomainEventSerializer.Deserialize(EventType, Data);
}
