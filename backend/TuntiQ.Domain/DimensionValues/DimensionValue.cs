namespace TuntiQ.Domain.Dimensions;

public sealed class DimensionValue : ModelBase, ITenantScoped, ILocalizedName
{
    public const int MaxNameLength = 200;

    public Guid TenantId { get; private set; }
    public Guid DimensionId { get; private set; }
    public Guid? ReferenceId { get; private set; } 
    public string NameEn { get; private set; } = null!;
    public string NameFi { get; private set; } = null!;
    public bool Enabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool Deleted => DeletedAt is not null;

    public DimensionValue(
        Guid id,
        Guid tenantId,
        Guid dimensionId,
        Guid? referenceId,
        string nameEn,
        string nameFi,
        bool enabled,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt) : base(id)
    {
        TenantId = tenantId;
        DimensionId = dimensionId;
        ReferenceId = referenceId;
        NameEn = nameEn;
        NameFi = nameFi;
        Enabled = enabled;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }

    public static DimensionValue Create(Dimension dimension, string nameEn, string nameFi, Guid? referenceId = null)
    {
        if (dimension.Deleted)
            throw ValidationException.Deleted();

        if (dimension.Kind == DimensionKinds.General && referenceId is not null)
            throw new InvalidOperationException("General dimension values cannot have a reference.");

        if (dimension.Kind != DimensionKinds.General && referenceId is null)
            throw new InvalidOperationException("Project and ticket dimension values must have a reference.");

        TenantMismatchException.ThrowIfMismatch(dimension.TenantId, CurrentUser.TenantId);
        ValidateNames(nameEn, nameFi);

        var value = new DimensionValue(
            Guid.CreateVersion7(),
            dimension.TenantId,
            dimension.Id,
            referenceId,
            nameEn.Trim(),
            nameFi.Trim(),
            enabled: true,
            DateTimeOffset.UtcNow,
            null);

        value.Raise(new DimensionValueCreated(
            value.Id,
            value.TenantId,
            value.DimensionId,
            value.ReferenceId,
            value.NameEn,
            value.NameFi,
            value.Enabled));

        return value;
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

        Raise(new DimensionValueRenamed(Id, TenantId, oldNameEn, oldNameFi, newNameEn, newNameFi));
    }

    public void SetEnabled(bool enabled)
    {
        EnsureNotDeleted();
        if (Enabled == enabled)
            return;

        Enabled = enabled;

        Raise(enabled ? new DimensionValueEnabled(Id, TenantId) : new DimensionValueDisabled(Id, TenantId));
    }

    public void Delete()
    {
        if (Deleted)
            return;

        DeletedAt = DateTimeOffset.UtcNow;

        Raise(new DimensionValueDeleted(Id, TenantId));
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