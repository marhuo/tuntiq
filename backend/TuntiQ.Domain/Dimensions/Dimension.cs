using TuntiQ.Domain.Dimensions;

public sealed class Dimension : ModelBase, ITenantScoped, ILocalizedName
{
    public const int MaxNameLength = 200;
    public Guid TenantId { get; private set; }
    public string NameEn { get; private set; } = null!;
    public string NameFi { get; private set; } = null!;

    public DimensionKinds Kind { get; private set; }
    public bool AllowMultiple { get; private set; }
    public bool IsMandatory { get; private set; }
    public bool Enabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool Deleted => DeletedAt is not null;

    public Dimension(
        Guid id,
        Guid tenantId,
        DimensionKinds kind,
        string nameEn,
        string nameFi,
        bool allowMultiple,
        bool isMandatory,
        bool enabled,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt) : base(id)
    {
        TenantId = tenantId;
        Kind = kind;
        NameEn = nameEn;
        NameFi = nameFi;
        AllowMultiple = allowMultiple;
        IsMandatory = isMandatory;
        Enabled = enabled;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }

    public static Dimension Create(
        Guid tenantId,
        DimensionKinds kind,
        string nameEn,
        string nameFi,
        bool allowMultiple,
        bool isMandatory)
    {
        ValidateNames(nameEn, nameFi);

        var now = DateTimeOffset.UtcNow;

        var dimension = new Dimension(
            Guid.CreateVersion7(),
            tenantId,
            kind,
            nameEn.Trim(),
            nameFi.Trim(),
            allowMultiple,
            isMandatory,
            enabled: true,
            now,
            null);

        dimension.Raise(new DimensionCreated(
            dimension.Id,
            dimension.TenantId,
            dimension.Kind,
            dimension.NameEn,
            dimension.NameFi,
            dimension.AllowMultiple,
            dimension.IsMandatory,
            dimension.Enabled));

        return dimension;
    }

    public void SetNames(string newNameEn, string newNameFi)
    {
        EnsureNotDeleted();
        ValidateNames(newNameEn, newNameFi);

        newNameEn = newNameEn.Trim();
        newNameFi = newNameFi.Trim();
        if (newNameEn == NameEn && newNameFi == NameFi)
            return;

        var oldNameEn = NameEn;
        var oldNameFi = NameFi;
        NameEn = newNameEn;
        NameFi = newNameFi;

        Raise(new DimensionRenamed(Id, TenantId, oldNameEn, oldNameFi, newNameEn, newNameFi));
    }

    public void SetEnabled(bool enabled)
    {
        EnsureNotDeleted();
        if (Enabled == enabled)
            return;

        Enabled = enabled;

        Raise(enabled
        ? new DimensionEnabled(Id, TenantId)
        : new DimensionDisabled(Id, TenantId));
    }

    public void SetAllowMultiple(bool allowMultiple)
    {
        EnsureNotDeleted();
        if (AllowMultiple == allowMultiple)
            return;

        AllowMultiple = allowMultiple;

        Raise(new DimensionAllowMultipleChanged(Id, TenantId, allowMultiple));
    }

    public void SetIsMandatory(bool isMandatory)
    {
        EnsureNotDeleted();
        if (IsMandatory == isMandatory)
            return;

        IsMandatory = isMandatory;

        Raise(new DimensionIsMandatoryChanged(Id, TenantId, isMandatory));
    }

    public void Delete()
    {
        if (Deleted)
            return;

        var now = DateTimeOffset.UtcNow;
        DeletedAt = now;

        Raise(new DimensionDeleted(Id, TenantId));
    }

    private static void ValidateNames(string nameEn, string nameFi)
    {
        var errors = new ErrorCollector();
        errors.Required(nameof(NameEn), nameEn, "Name (English)", "Nimi (englanti)");
        errors.Required(nameof(NameFi), nameFi, "Name (Finnish)", "Nimi (suomi)");
        errors.MaxLength(nameof(NameEn), nameEn, "Name (English)", "Nimi (englanti)", 0, MaxNameLength);
        errors.MaxLength(nameof(NameFi), nameFi, "Name (Finnish)", "Nimi (suomi)", 0, MaxNameLength);
        errors.ThrowIfAny();
    }

    private void EnsureNotDeleted()
    {
        if (Deleted)
            throw ValidationException.Deleted();
    }
}
public enum DimensionKinds
{
    General,
    Project, 
    Ticket
}
