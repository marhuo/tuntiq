namespace TuntiQ.Domain.Common;

public sealed class TenantMismatchException(Guid expectedTenantId, Guid? actualTenantId)
    : Exception($"Tenant mismatch. Expected {expectedTenantId}, got {actualTenantId}.")
{
    public Guid ExpectedTenantId { get; } = expectedTenantId;
    public Guid? ActualTenantId { get; } = actualTenantId;

    public static void ThrowIfMismatch(Guid expectedTenantId, Guid? actualTenantId)
    {
        if (expectedTenantId != actualTenantId)
            throw new TenantMismatchException(expectedTenantId, actualTenantId);
    }
}