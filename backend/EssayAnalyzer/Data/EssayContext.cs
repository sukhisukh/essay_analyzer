using Microsoft.EntityFrameworkCore;
using EssayAnalyzer.Models;

public class EssayContext : DbContext
{
    public EssayContext(DbContextOptions<EssayContext> options)
        : base(options) { }

    public DbSet<Essay> Essays { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<SpellCheckLog> SpellCheckLogs { get; set; }
}