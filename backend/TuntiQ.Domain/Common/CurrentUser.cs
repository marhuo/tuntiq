public sealed record UserContext(Guid UserId, Guid? TenantId, bool IsSystemUser)
{
    public Guid BatchId { get; } = Guid.CreateVersion7();

    public static UserContext System => new(Guid.Empty, null, true);
}

public static class CurrentUser
{
    private static readonly AsyncLocal<UserContext?> Current = new();

    public static void Set(UserContext user) => Current.Value = user;

    public static UserContext? TryGet() => Current.Value;

    public static void Clear() => Current.Value = null;

    public static Guid UserId => Current.Value?.UserId ?? throw new InvalidOperationException("Current user is not set.");

    public static Guid? TenantId => Current.Value?.TenantId;

    public static bool IsSystemUser => Current.Value?.IsSystemUser ?? false;

    public static Guid? BatchId => Current.Value?.BatchId;
}
