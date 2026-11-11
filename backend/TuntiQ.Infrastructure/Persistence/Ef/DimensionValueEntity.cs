using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;

namespace TuntiQ.Infrastructure.Persistence.Ef;

public class DimensionValueEntity : ITenantScopedEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DimensionId { get; set; }
    public Guid? ReferenceId { get; set; }
    public List<DimensionValueNameEntity> Names { get; set; } = [];
    public bool Enabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool Deleted { get; set; }

    public static DimensionValueEntity FromDomain(DimensionValue value)
    {
        var entity = new DimensionValueEntity
        {
            Id = value.Id,
            TenantId = value.TenantId,
            DimensionId = value.DimensionId,
            ReferenceId = value.ReferenceId
        };
        entity.UpdateFrom(value);
        return entity;
    }

    public void UpdateFrom(DimensionValue value)
    {
        Names.SetName(TenantId, "en", value.NameEn);
        Names.SetName(TenantId, "fi", value.NameFi);
        Enabled = value.Enabled;
        CreatedAt = value.CreatedAt;
        DeletedAt = value.DeletedAt;
        Deleted = value.Deleted;
    }

    public DimensionValue ToDomain() =>
        new(
            id: Id,
            tenantId: TenantId,
            dimensionId: DimensionId,
            referenceId: ReferenceId,
            nameEn: Names.GetName("en"),
            nameFi: Names.GetName("fi"),
            enabled: Enabled,
            createdAt: CreatedAt,
            deletedAt: DeletedAt);
}

public class DimensionValueEventEntity : EventEntityBase, ITenantScopedEntity
{
    public Guid DimensionValueId { get; set; }
    public Guid TenantId { get; set; }

    public static DimensionValueEventEntity FromDomain(Guid dimensionValueId, Guid tenantId, IDomainEvent domainEvent)
    {
        var entity = new DimensionValueEventEntity { DimensionValueId = dimensionValueId, TenantId = tenantId };
        entity.SetFrom(domainEvent);
        return entity;
    }
}

public class DimensionValueNameEntity : LocalizedNameEntityBase
{
    public Guid DimensionValueId { get; set; }
}