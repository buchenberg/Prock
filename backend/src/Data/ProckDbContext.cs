using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace backend.Data;

public class ProckDbContext : DbContext
{
    public DbSet<MockRoute> MockRoutes { get; init; }
    public DbSet<ProckConfig> ProckConfig { get; init; }
    public DbSet<OpenApiSpecification> OpenApiDocuments { get; init; }
    public static ProckDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<ProckDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);
    public ProckDbContext(DbContextOptions options)
        : base(options)
    {
        // Disable automatic transactions as standalone MongoDB does not support them
        try 
        {
            Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        }
        catch
        {
            // safely ignore if already set or not applicable
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure collections
        modelBuilder.Entity<MockRoute>().ToCollection("mockRoutes");
        modelBuilder.Entity<ProckConfig>().ToCollection("prockConfig");
        modelBuilder.Entity<OpenApiSpecification>().ToCollection("openApiDocuments");
        modelBuilder.Entity<OpenApiSpecification>().OwnsMany(x => x.Paths);
        modelBuilder.Entity<OpenApiSpecification>().HasKey(x => x._id);

        // Configure MockRoute entity
        modelBuilder.Entity<MockRoute>(entity =>
        {
            entity.HasKey(e => e._id);
            entity.Property(e => e.RouteId);
        });

        // Configure ProckConfig entity
        modelBuilder.Entity<ProckConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }

    public async Task<ProckConfig?> GetProckConfigAsync(CancellationToken cancellationToken = default)
    {
        return await ProckConfig.SingleOrDefaultAsync(x => x.Id != Guid.Empty, cancellationToken);
    }

    public async Task<List<OpenApiSpecification>> GetActiveOpenApiDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await OpenApiDocuments.Where(x => x.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<OpenApiSpecification?> GetOpenApiDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await OpenApiDocuments.SingleOrDefaultAsync(x => x.DocumentId == documentId, cancellationToken);
    }

    public async Task<OpenApiSpecification?> GetOpenApiDocumentByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        return await OpenApiDocuments.SingleOrDefaultAsync(x => x.Title == title && x.IsActive, cancellationToken);
    }

}
