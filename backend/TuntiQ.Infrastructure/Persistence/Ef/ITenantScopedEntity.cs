namespace TuntiQ.Infrastructure.Persistence.Ef;

public interface ITenantScopedEntity
{
    Guid TenantId { get; set; }
}
