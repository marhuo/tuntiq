using TuntiQ.Infrastructure.Persistence.Ef;
public abstract class LocalizedNameEntityBase : ITenantScopedEntity
{
    public const int MaxNameLength = 300;

    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Language { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public static class LocalizedNameEntityExtensions
{
    public static string GetName<T>(this IEnumerable<T> names, string language)
        where T : LocalizedNameEntityBase =>
        names.Single(n => n.Language == language).Name;

    public static void SetName<T>(this List<T> names, Guid tenantId, string language, string name)
        where T : LocalizedNameEntityBase, new()
    {
        var existing = names.SingleOrDefault(n => n.Language == language);
        if (existing is not null)
        {
            existing.Name = name;
            return;
        }

        names.Add(new T { Id = Guid.CreateVersion7(), TenantId = tenantId, Language = language, Name = name });
    }
}