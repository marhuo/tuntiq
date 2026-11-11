namespace TuntiQ.Domain.Dimensions;

public sealed record DimensionCreated(
    Guid DimensionId,
    Guid TenantId,
    DimensionKinds Kind,
    string NameEn,
    string NameFi,
    bool AllowMultiple,
    bool IsMandatory,
    bool Enabled) : DomainEvent;

public sealed record DimensionRenamed(
    Guid DimensionId,
    Guid TenantId,
    string OldNameEn,
    string OldNameFi,
    string NewNameEn,
    string NewNameFi) : DomainEvent;

public sealed record DimensionEnabled(Guid DimensionId, Guid TenantId) : DomainEvent;

public sealed record DimensionDisabled(Guid DimensionId, Guid TenantId) : DomainEvent;

public sealed record DimensionDeleted(Guid DimensionId, Guid TenantId) : DomainEvent;

public sealed record DimensionAllowMultipleChanged(Guid DimensionId, Guid TenantId, bool AllowMultiple) : DomainEvent;

public sealed record DimensionIsMandatoryChanged(Guid DimensionId, Guid TenantId, bool IsMandatory) : DomainEvent;