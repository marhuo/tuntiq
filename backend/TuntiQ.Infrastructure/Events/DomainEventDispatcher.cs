using Microsoft.Extensions.DependencyInjection;
using TuntiQ.Domain.Common;

namespace TuntiQ.Infrastructure.Events;

public class DomainEventDispatcher(IServiceProvider services) : IDomainEventDispatcher
{
    public async Task Dispatch(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handle = handlerType.GetMethod("Handle")!;

            foreach (var handler in services.GetServices(handlerType))
                await (Task)handle.Invoke(handler, [domainEvent])!;
        }
    }
}