using Microsoft.EntityFrameworkCore;
using EssayAnalyzer.Models;

namespace EssayAnalyzer.Data
{
    // Add these DbSets to your existing ApplicationDbContext
    // If your existing context is named differently, update accordingly

    public partial class ApplicationDbContext : DbContext
    {
        // ── Spell Checker tables ─────────────────────────────────
        public DbSet<School> Schools { get; set; }
        public DbSet<SpellCheckLog> SpellCheckLogs { get; set; }

        protected void OnSpellCheckerModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<School>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LicenseKey).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<SpellCheckLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.School)
                      .WithMany(s => s.Logs)
                      .HasForeignKey(e => e.SchoolId);
                entity.HasIndex(e => new { e.SchoolId, e.Year, e.Month });
                entity.HasIndex(e => e.LicenseKey);
                entity.Property(e => e.CheckedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
