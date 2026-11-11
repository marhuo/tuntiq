namespace TuntiQ.Domain.Common;

public interface ITenantScoped
{
    Guid TenantId { get; }
}
