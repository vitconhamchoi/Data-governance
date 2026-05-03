using Microsoft.EntityFrameworkCore;
using PolicyService.Models;

namespace PolicyService.Data
{
    public class PolicyDbContext : DbContext
    {
        public PolicyDbContext(DbContextOptions<PolicyDbContext> options) : base(options)
        {
        }

        public DbSet<Policy> Policies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Dataset).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Column).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Rule).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Create index for fast lookup
                entity.HasIndex(e => new { e.Dataset, e.Column, e.Role });
            });
        }
    }
}
