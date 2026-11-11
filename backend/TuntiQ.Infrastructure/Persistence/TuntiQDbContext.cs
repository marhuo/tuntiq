using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using TuntiQ.Infrastructure.Persistence.Ef;

namespace TuntiQ.Infrastructure.Persistence;

public class TuntiQDbContext(DbContextOptions<TuntiQDbContext> options) : DbContext(options)
{
    private Guid? CurrentTenantId => CurrentUser.TenantId;
    private bool IsSystemUser => CurrentUser.IsSystemUser;

    public DbSet<DimensionEntity> Dimensions => Set<DimensionEntity>();
    public DbSet<DimensionEventEntity> DimensionEvents => Set<DimensionEventEntity>();

    public DbSet<DimensionValueEntity> DimensionValues => Set<DimensionValueEntity>();
    public DbSet<DimensionValueEventEntity> DimensionValueEvents => Set<DimensionValueEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Entity<DimensionEntity>(b =>
        {
            b.ToTable("dimensions");
            b.HasMany(e => e.Names).WithOne().HasForeignKey(n => n.DimensionId);
            b.HasIndex(e => e.TenantId).HasFilter("deleted = false");
        });

        modelBuilder.Entity<DimensionNameEntity>(b =>
        {
            b.ToTable("dimension_names");
            ConfigureLocalizedName(b);
            b.HasIndex(e => new { e.DimensionId, e.Language }).IsUnique();
        });

        modelBuilder.Entity<DimensionEventEntity>(b =>
        {
            b.ToTable("dimension_events");
            b.Property(e => e.EventType).HasMaxLength(200);
            b.Property(e => e.Data).HasColumnType("jsonb");
            b.HasIndex(e => e.DimensionId);
            b.HasOne<DimensionEntity>().WithMany().HasForeignKey(e => e.DimensionId);
        });

        modelBuilder.Entity<DimensionValueEntity>(b =>
        {
            b.ToTable("dimension_values");
            b.HasOne<DimensionEntity>().WithMany().HasForeignKey(e => e.DimensionId);
            b.HasMany(e => e.Names).WithOne().HasForeignKey(n => n.DimensionValueId);
            b.HasIndex(e => new { e.TenantId, e.Enabled }).HasFilter("deleted = false");
            b.HasIndex(e => new { e.TenantId, e.ReferenceId })
             .IsUnique()
             .HasFilter("deleted = false AND reference_id IS NOT NULL");
        });

        modelBuilder.Entity<DimensionValueNameEntity>(b =>
        {
            b.ToTable("dimension_value_names");
            ConfigureLocalizedName(b);
            b.HasIndex(e => new { e.DimensionValueId, e.Language }).IsUnique();
        });

        modelBuilder.Entity<DimensionValueEventEntity>(b =>
        {
            b.ToTable("dimension_value_events");
            b.Property(e => e.EventType).HasMaxLength(200);
            b.Property(e => e.Data).HasColumnType("jsonb");
            b.HasIndex(e => e.DimensionValueId);
            b.HasOne<DimensionValueEntity>().WithMany().HasForeignKey(e => e.DimensionValueId);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var type = entityType.ClrType;

            if (typeof(ITenantScopedEntity).IsAssignableFrom(type))
                Invoke(nameof(ApplyTenantFilter), type, modelBuilder);

            if (typeof(ISoftDeletableEntity).IsAssignableFrom(type))
                Invoke(nameof(ApplySoftDeleteFilter), type, modelBuilder);
        }
    }

    private void Invoke(string method, Type entityType, ModelBuilder modelBuilder) =>
        GetType()
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(entityType)
            .Invoke(this, [modelBuilder]);

    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantScopedEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter("Tenant", x => IsSystemUser || x.TenantId == CurrentTenantId);

        var index = modelBuilder.Entity<T>().HasIndex(x => x.TenantId);
        if (typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(T)))
            index.HasFilter("deleted = false");
    }

    private void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : class, ISoftDeletableEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter("SoftDelete", x => !x.Deleted);
    }

    private static void ConfigureLocalizedName<T>(EntityTypeBuilder<T> b) where T : LocalizedNameEntityBase
    {
        b.Property(e => e.Language).IsRequired().HasMaxLength(10);
        b.Property(e => e.Name).IsRequired().HasMaxLength(LocalizedNameEntityBase.MaxNameLength);

        b.Property<string>("NameSearch")
         .HasComputedColumnSql("lower(name)", stored: true)
         .UseCollation("C");

        b.HasIndex("TenantId", "Language", "NameSearch");
    }
}