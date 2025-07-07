using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace backend.Data;

public class ProckDbContext : DbContext
{
    public DbSet<MockRoute> MockRoutes { get; init; }
    public DbSet<ProckConfig> ProckConfig { get; init; }
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
        modelBuilder.Entity<MockRoute>().ToCollection("mockRoutes");
        modelBuilder.Entity<ProckConfig>().ToCollection("prockConfig");
    }

    public async Task<ProckConfig?> GetProckConfigAsync(CancellationToken cancellationToken = default)
    {
        return await ProckConfig.SingleOrDefaultAsync(x => x.Id != Guid.Empty, cancellationToken);
    }
    
}