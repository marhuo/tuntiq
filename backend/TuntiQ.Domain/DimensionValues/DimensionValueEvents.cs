namespace TuntiQ.Domain.Dimensions;

public sealed record DimensionValueCreated(
    Guid DimensionValueId,
    Guid TenantId,
    Guid DimensionId,
    Guid? ReferenceId,
    string NameEn,
    string NameFi,
    bool Enabled) : DomainEvent;

public sealed record DimensionValueRenamed(
    Guid DimensionValueId,
    Guid TenantId,
    string OldNameEn,
    string OldNameFi,
    string NewNameEn,
    string NewNameFi) : DomainEvent;

public sealed record DimensionValueEnabled(Guid DimensionValueId, Guid TenantId) : DomainEvent;

public sealed record DimensionValueDisabled(Guid DimensionValueId, Guid TenantId) : DomainEvent;

public sealed record DimensionValueDeleted(Guid DimensionValueId, Guid TenantId) : DomainEvent;