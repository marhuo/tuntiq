using TuntiQ.Domain.Common;

namespace TuntiQ.Infrastructure.Persistence.Ef;

public class DimensionEntity : ITenantScopedEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DimensionKinds Kind { get; set; }
    public List<DimensionNameEntity> Names { get; set; } = [];
    public bool AllowMultiple { get; set; }
    public bool IsMandatory { get; set; }
    public bool Enabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool Deleted { get; set; }

    public static DimensionEntity FromDomain(Dimension dimension)
    {
        var entity = new DimensionEntity
        {
            Id = dimension.Id,
            TenantId = dimension.TenantId,
            Kind = dimension.Kind
        };
        entity.UpdateFrom(dimension);
        return entity;
    }

    public void UpdateFrom(Dimension dimension)
    {
        Names.SetName(TenantId, "en", dimension.NameEn);
        Names.SetName(TenantId, "fi", dimension.NameFi);
        AllowMultiple = dimension.AllowMultiple;
        IsMandatory = dimension.IsMandatory;
        Enabled = dimension.Enabled;
        CreatedAt = dimension.CreatedAt;
        DeletedAt = dimension.DeletedAt;
        Deleted = dimension.Deleted;
    }

    public Dimension ToDomain() =>
        new(
            id: Id,
            tenantId: TenantId,
            kind: Kind,
            nameEn: Names.GetName("en"),
            nameFi: Names.GetName("fi"),
            allowMultiple: AllowMultiple,
            isMandatory: IsMandatory,
            enabled: Enabled,
            createdAt: CreatedAt,
            deletedAt: DeletedAt);
}

public class DimensionEventEntity : EventEntityBase, ITenantScopedEntity
{
    public Guid DimensionId { get; set; }
    public Guid TenantId { get; set; }

    public static DimensionEventEntity FromDomain(Guid dimensionId, Guid tenantId, IDomainEvent domainEvent)
    {
        var entity = new DimensionEventEntity
        {
            DimensionId = dimensionId,
            TenantId = tenantId
        };
        entity.SetFrom(domainEvent);
        return entity;
    }
}

public class DimensionNameEntity : LocalizedNameEntityBase
{
    public Guid DimensionId { get; set; }
}