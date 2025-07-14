
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
        modelBuilder.Entity<Widget>()
            .ToTable("Widgets");
        modelBuilder.Entity<Widget>()
            .HasKey(b => b.Id);
        modelBuilder.Entity<Widget>()
            .Property(b => b.Name)
            .IsRequired();
        modelBuilder.Entity<Widget>()
            .Property(b => b.Description)
            .IsRequired();
    }

    public virtual DbSet<Prock.Backend.src.Data.MariaDb.Widget> Widgets { get; set; }
    public virtual DbSet<Prock.Backend.src.Data.MariaDb.MockRoute> MockRoutes { get; set; }
}