using System.Text.Json;
using TuntiQ.Domain.Common;

namespace TuntiQ.Infrastructure.Persistence.Ef;

/// <summary>
/// Serializes domain events for the audit log. Events are write-only: they are never
/// deserialized back into event types, only read as raw JSON.
/// </summary>
public static class DomainEventSerializer
{
    public static string GetTypeName(IDomainEvent domainEvent) =>
        domainEvent.GetType().Name;

    public static string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
}