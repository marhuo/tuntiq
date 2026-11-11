using System.Text.Json;
using TuntiQ.Domain.Common;

namespace TuntiQ.Infrastructure.Persistence.Ef;

public static class DomainEventSerializer
{
    private static readonly Dictionary<string, Type> EventTypes =
        typeof(IDomainEvent).Assembly
            .GetTypes()
            .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .ToDictionary(t => t.Name);

    public static string GetTypeName(IDomainEvent domainEvent) =>
        domainEvent.GetType().Name;

    public static string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

    public static IDomainEvent Deserialize(string eventType, string data)
    {
        if (!EventTypes.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"Unknown event type: {eventType}");

        return (IDomainEvent)JsonSerializer.Deserialize(data, type)!;
    }
}