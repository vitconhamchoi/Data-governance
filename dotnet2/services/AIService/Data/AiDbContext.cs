using AIService.Models;
using Microsoft.EntityFrameworkCore;

namespace AIService.Data
{
    public class AiDbContext : DbContext
    {
        public AiDbContext(DbContextOptions<AiDbContext> options) : base(options) { }

        public DbSet<PolicyRecommendation> PolicyRecommendations { get; set; }
        public DbSet<MetadataEmbedding> MetadataEmbeddings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyRecommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ai_policy_recommendations");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TableName).HasColumnName("table_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.ColumnName).HasColumnName("column_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Tag).HasColumnName("tag").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Rule).HasColumnName("rule").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasColumnName("reason").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
                entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(100);
            });

            modelBuilder.Entity<MetadataEmbedding>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("metadata_embeddings");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.EntityType).HasColumnName("entity_type").IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityName).HasColumnName("entity_name").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasColumnName("description").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                // embedding column is vector(1536) — managed via raw SQL / VectorSearchService
                entity.Ignore("Embedding");
                entity.HasIndex(e => new { e.EntityType, e.EntityName }).IsUnique();
            });
        }
    }
}
