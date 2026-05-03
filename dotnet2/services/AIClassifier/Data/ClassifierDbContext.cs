using AIClassifier.Models;
using Microsoft.EntityFrameworkCore;

namespace AIClassifier.Data
{
    public class ClassifierDbContext : DbContext
    {
        public ClassifierDbContext(DbContextOptions<ClassifierDbContext> options) : base(options) { }

        public DbSet<ColumnTag> ColumnTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ColumnTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("column_tags");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TableName).HasColumnName("table_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.ColumnName).HasColumnName("column_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Tag).HasColumnName("tag").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Confidence).HasColumnName("confidence");
                entity.Property(e => e.DetectionMethod).HasColumnName("detection_method").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => new { e.TableName, e.ColumnName });
            });
        }
    }
}
