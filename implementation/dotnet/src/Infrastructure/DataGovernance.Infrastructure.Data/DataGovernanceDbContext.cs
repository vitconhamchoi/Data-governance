using DataGovernance.Domain.Entities;
using DataGovernance.Domain.Entities.Harness;
using Microsoft.EntityFrameworkCore;

namespace DataGovernance.Infrastructure.Data;

/// <summary>
/// Database context for Data Governance platform
/// </summary>
public class DataGovernanceDbContext : DbContext
{
    public DataGovernanceDbContext(DbContextOptions<DataGovernanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataAsset> DataAssets { get; set; } = null!;
    public DbSet<DataLineage> DataLineages { get; set; } = null!;
    public DbSet<DataQualityRule> DataQualityRules { get; set; } = null!;
    public DbSet<DataQualityResult> DataQualityResults { get; set; } = null!;
    public DbSet<GovernancePolicy> GovernancePolicies { get; set; } = null!;
    public DbSet<PolicyViolation> PolicyViolations { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Run> Runs { get; set; } = null!;
    public DbSet<RunStep> RunSteps { get; set; } = null!;
    public DbSet<Approval> Approvals { get; set; } = null!;
    public DbSet<Memory> Memories { get; set; } = null!;
    public DbSet<ToolDefinition> ToolDefinitions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataGovernanceDbContext).Assembly);

        // Configure schema
        modelBuilder.HasDefaultSchema("governance");

        // Configure DataAsset
        modelBuilder.Entity<DataAsset>(entity =>
        {
            entity.ToTable("data_assets");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QualifiedName).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.Platform, e.AssetType });
            entity.HasIndex(e => e.Zone);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.QualifiedName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Platform).HasMaxLength(100);
            entity.Property(e => e.Uri).HasMaxLength(1000);
            entity.Property(e => e.Owner).HasMaxLength(256);
            entity.Property(e => e.Steward).HasMaxLength(256);

            // Configure JSON columns for PostgreSQL
            entity.Property(e => e.Tags)
                .HasColumnType("jsonb");

            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");

            entity.Property(e => e.Schema)
                .HasColumnType("jsonb");

            // Soft delete filter
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure DataLineage
        modelBuilder.Entity<DataLineage>(entity =>
        {
            entity.ToTable("data_lineages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SourceAssetId);
            entity.HasIndex(e => e.TargetAssetId);
            entity.HasIndex(e => new { e.SourceAssetId, e.TargetAssetId });

            entity.Property(e => e.Process).HasMaxLength(256);

            entity.Property(e => e.ColumnMappings)
                .HasColumnType("jsonb");

            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");
        });

        // Configure DataQualityRule
        modelBuilder.Entity<DataQualityRule>(entity =>
        {
            entity.ToTable("data_quality_rules");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DataAssetId);
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.RuleDefinition).IsRequired();
        });

        // Configure DataQualityResult
        modelBuilder.Entity<DataQualityResult>(entity =>
        {
            entity.ToTable("data_quality_results");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QualityRuleId);
            entity.HasIndex(e => e.ExecutedAt);

            entity.Property(e => e.Metrics)
                .HasColumnType("jsonb");
        });

        // Configure GovernancePolicy
        modelBuilder.Entity<GovernancePolicy>(entity =>
        {
            entity.ToTable("governance_policies");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PolicyDefinition).IsRequired();
            entity.Property(e => e.Scope).HasMaxLength(256);

            entity.Property(e => e.ApplicableAssets)
                .HasColumnType("jsonb");

            entity.Property(e => e.Tags)
                .HasColumnType("jsonb");
        });

        // Configure PolicyViolation
        modelBuilder.Entity<PolicyViolation>(entity =>
        {
            entity.ToTable("policy_violations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PolicyId);
            entity.HasIndex(e => e.DataAssetId);
            entity.HasIndex(e => e.ViolatedAt);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Actor).HasMaxLength(256);
            entity.Property(e => e.Action).HasMaxLength(256);
        });

        // Configure AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Actor);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.RequestId);

            entity.Property(e => e.ResourceType).HasMaxLength(100);
            entity.Property(e => e.ResourceName).HasMaxLength(512);
            entity.Property(e => e.Actor).HasMaxLength(256);
            entity.Property(e => e.ActorType).HasMaxLength(50);
            entity.Property(e => e.Action).HasMaxLength(256);

            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");

            // Partition by CreatedAt for performance (time-series data)
            // Note: Actual partitioning would be done via migrations
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Channel).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Role);

            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(50);

            entity.HasOne(e => e.Session)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Run>(entity =>
        {
            entity.ToTable("runs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.TenantId, e.Status });

            entity.Property(e => e.TaskType).HasMaxLength(100);
            entity.Property(e => e.Strategy).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.CostUsd).HasPrecision(18, 6);

            entity.HasOne(e => e.Session)
                .WithMany(e => e.Runs)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RunStep>(entity =>
        {
            entity.ToTable("run_steps");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.RunId);
            entity.HasIndex(e => new { e.RunId, e.StepNo }).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.StepType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ToolName).HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ErrorCode).HasMaxLength(100);

            entity.HasOne(e => e.Run)
                .WithMany(e => e.Steps)
                .HasForeignKey(e => e.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Approval>(entity =>
        {
            entity.ToTable("approvals");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.RunId);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.ActionType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RequestedBy).HasMaxLength(256);
            entity.Property(e => e.ResolvedBy).HasMaxLength(256);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);

            entity.HasOne(e => e.Run)
                .WithMany(e => e.Approvals)
                .HasForeignKey(e => e.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Memory>(entity =>
        {
            entity.ToTable("memories");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.Scope, e.ScopeId });
            entity.HasIndex(e => e.MemoryType);

            entity.Property(e => e.Scope).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MemoryType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
        });

        modelBuilder.Entity<ToolDefinition>(entity =>
        {
            entity.ToTable("tool_definitions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => e.SideEffectLevel);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.InputSchema).HasColumnType("jsonb");
            entity.Property(e => e.OutputSchema).HasColumnType("jsonb");
            entity.Property(e => e.SideEffectLevel).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.PermissionLevel).HasConversion<string>().HasMaxLength(50);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.Version++;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
