using AutoFixture;
using Backend.Infrastructure.Data.Context;
using Backend.Core.Domain.Entities;
using Backend.Core.Domain.Entities.OpenApi;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace backend.Tests.TestBase;

/// <summary>
/// Test helper for creating in-memory database contexts for unit tests
/// </summary>
public static class TestDbContext
{
    /// <summary>
    /// Creates a ProckDbContext with an in-memory database for testing
    /// </summary>
    public static ProckDbContext CreateInMemory(string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<ProckDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ProckDbContext(options);
    }

    /// <summary>
    /// Creates a ProckDbContext and seeds it with test data
    /// </summary>
    public static async Task<ProckDbContext> CreateSeededAsync(IFixture fixture, string? databaseName = null)
    {
        var context = CreateInMemory(databaseName);
        
        // Seed with test data
        var mockRoutes = fixture.CreateMany<MockRoute>(3).ToList();
        var prockConfig = fixture.Create<ProckConfig>();
        var openApiDocs = fixture.CreateMany<OpenApiSpecification>(2).ToList();

        context.MockRoutes.AddRange(mockRoutes);
        context.ProckConfig.Add(prockConfig);
        context.OpenApiDocuments.AddRange(openApiDocs);
        
        await context.SaveChangesAsync();
        
        // Clear change tracker to simulate fresh queries
        context.ChangeTracker.Clear();
        
        return context;
    }

    /// <summary>
    /// Creates a ProckDbContext with specific test entities
    /// </summary>
    public static async Task<ProckDbContext> CreateWithEntitiesAsync(params object[] entities)
    {
        var context = CreateInMemory();
        
        foreach (var entity in entities)
        {
            context.Add(entity);
        }
        
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        return context;
    }
}
