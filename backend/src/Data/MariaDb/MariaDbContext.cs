
using Prock.Backend.src.Data.MariaDb;
using Microsoft.EntityFrameworkCore;

public partial class MariaDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MariaDbContext(DbContextOptions<MariaDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the entity mappings here if needed
        base.OnModelCreating(modelBuilder);
    }


    public virtual DbSet<MockRoute> MockRoutes { get; set; }
}