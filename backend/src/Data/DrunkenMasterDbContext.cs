using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace backend.Data;

public class DrunkenMasterDbContext : DbContext
{
    public DbSet<MockRoute> MockRoutes { get; init; }
    public static DrunkenMasterDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<DrunkenMasterDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);
    public DrunkenMasterDbContext(DbContextOptions options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MockRoute>().ToCollection("mockRoutes");
    }
    
}