using Microsoft.EntityFrameworkCore;

public class EssayContext : DbContext
{
    public EssayContext(DbContextOptions<EssayContext> options)
        : base(options) { }

    public DbSet<Essay> Essays { get; set; }
}