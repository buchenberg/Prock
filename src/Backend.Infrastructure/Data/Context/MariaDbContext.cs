using Backend.Core.Domain.Entities.MariaDb;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data.Context;

public class MariaDbContext : DbContext
{
    public MariaDbContext(DbContextOptions<MariaDbContext> options) : base(options)
    {
    }

    public DbSet<MockRoute> MockRoutes { get; set; }
    public DbSet<ProckConfig> ProckConfigs { get; set; }
    public DbSet<OpenApiSpecification> OpenApiSpecifications { get; set; }
    public DbSet<OpenApiPath> OpenApiPaths { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MockRoute
        modelBuilder.Entity<MockRoute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RouteId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Method).HasMaxLength(10);
            entity.Property(e => e.Path).HasMaxLength(500);
            entity.Property(e => e.Mock).HasColumnType("TEXT");
            entity.HasIndex(e => e.RouteId).IsUnique();
        });


        // Configure ProckConfig
        modelBuilder.Entity<ProckConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Host).HasMaxLength(255);
            entity.Property(e => e.Port).HasMaxLength(10);
            entity.Property(e => e.UpstreamUrl).HasMaxLength(500);
            entity.Property(e => e.MongoDbUri).HasMaxLength(500);
            entity.Property(e => e.DbName).HasMaxLength(255);
        });

        // Configure OpenApiSpecification
        modelBuilder.Entity<OpenApiSpecification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Version).HasMaxLength(50);
            entity.Property(e => e.OpenApiVersion).HasMaxLength(10);
            entity.Property(e => e.Content).IsRequired().HasColumnType("LONGTEXT");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure OpenApiPath
        modelBuilder.Entity<OpenApiPath>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Summary).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.OperationId).HasMaxLength(255);
            entity.Property(e => e.RequestBody).HasColumnType("TEXT");
            entity.Property(e => e.Responses).HasColumnType("TEXT");
            entity.Property(e => e.Parameters).HasColumnType("TEXT");
            
            // Foreign key relationship
            entity.HasOne(e => e.OpenApiSpecification)
                .WithMany()
                .HasForeignKey(e => e.OpenApiSpecificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
