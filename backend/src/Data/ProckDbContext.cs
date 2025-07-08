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
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure collections
        modelBuilder.Entity<MockRoute>().ToCollection("mockRoutes");
        modelBuilder.Entity<ProckConfig>().ToCollection("prockConfig");
        modelBuilder.Entity<OpenApiSpecification>().ToCollection("openApiDocuments");
        
        // Configure OpenApiDocument entity specifically for MongoDB
        modelBuilder.Entity<OpenApiSpecification>(entity =>
        {
            entity.HasKey(e => e._id);
            entity.Property(e => e.DocumentId);
            entity.Property(e => e.Title);
            entity.Property(e => e.Version);
            entity.Property(e => e.Description);
            entity.Property(e => e.OpenApiVersion);
            entity.Property(e => e.BasePath);
            entity.Property(e => e.Host);
            entity.Property(e => e.Schemes);
            entity.Property(e => e.Consumes);
            entity.Property(e => e.Produces);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.OriginalJson);
            // Ignore BsonDocument properties as they're not supported by EF Core
            entity.Ignore(e => e.PathsData);
            entity.Ignore(e => e.ComponentsData);
            entity.Ignore(e => e.TagsData);
            entity.Ignore(e => e.ServersData);
            entity.Ignore(e => e.ExternalDocsData);
        });
        
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