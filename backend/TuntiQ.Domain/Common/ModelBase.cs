namespace TuntiQ.Domain.Common;

public abstract class ModelBase
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => [.. _domainEvents];

    protected ModelBase(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id ei voi olla tyhjä.", nameof(id));

        Id = id;
    }

    protected void Raise(DomainEvent domainEvent, DateTimeOffset? occurredAt = null) =>
        _domainEvents.Add(Stamp(domainEvent) with { OccurredAt = occurredAt ?? DateTimeOffset.UtcNow });

    protected virtual DomainEvent Stamp(DomainEvent domainEvent) =>
        domainEvent with
        {
            UserId = CurrentUser.TryGet()?.UserId,
            BatchId = CurrentUser.BatchId
        };

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj) =>
        obj is ModelBase other
        && other.GetType() == GetType()
        && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(ModelBase? a, ModelBase? b) => Equals(a, b);

    public static bool operator !=(ModelBase? a, ModelBase? b) => !Equals(a, b);
}