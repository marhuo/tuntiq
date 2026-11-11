namespace TuntiQ.Domain.Common;

public interface IDomainEventDispatcher
{
    Task Dispatch(IEnumerable<IDomainEvent> domainEvents);
}