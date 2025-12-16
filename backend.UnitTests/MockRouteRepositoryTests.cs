using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Xunit;

namespace backend.UnitTests;

public class MockRouteRepositoryTests
{
    private readonly ProckDbContext _context;
    private readonly MockRouteRepository _repository;

    public MockRouteRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProckDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProckDbContext(options);
        _repository = new MockRouteRepository(_context);
    }

    [Fact]
    public async Task CreateRouteAsync_AddsRoute()
    {
        var dto = new MockRouteDto { Path = "/test", Method = "GET", HttpStatusCode = 200 };
        var result = await _repository.CreateRouteAsync(dto);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.RouteId);

        var saved = await _context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == result.RouteId);
        Assert.NotNull(saved);
        Assert.Equal("/test", saved.Path);
    }

    [Fact]
    public async Task CreateRoutesAsync_AddsRoutes()
    {
        var dtos = new List<MockRouteDto>
        {
            new MockRouteDto { Path = "/one", Method = "GET" },
            new MockRouteDto { Path = "/two", Method = "POST" }
        };

        await _repository.CreateRoutesAsync(dtos);

        var count = await _context.MockRoutes.CountAsync();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task UpdateRouteAsync_Updates()
    {
        var route = new MockRoute { _id = ObjectId.GenerateNewId(), RouteId = Guid.NewGuid(), Path = "/old", Method = "GET", Enabled = true };
        _context.MockRoutes.Add(route);
        await _context.SaveChangesAsync();

        var update = new MockRouteDto { RouteId = route.RouteId, Path = "/new", Method = "POST", HttpStatusCode = 201 };
        var result = await _repository.UpdateRouteAsync(update);

        Assert.NotNull(result);
        Assert.Equal("/new", result.Path);

        var saved = await _context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == route.RouteId);
        Assert.Equal("/new", saved.Path);
        Assert.Equal("POST", saved.Method);
    }

    [Fact]
    public async Task DeleteRouteAsync_Removes()
    {
        var route = new MockRoute { _id = ObjectId.GenerateNewId(), RouteId = Guid.NewGuid(), Path = "/delete", Method = "DELETE" };
        _context.MockRoutes.Add(route);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteRouteAsync(route.RouteId);
        Assert.True(result);

        var saved = await _context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == route.RouteId);
        Assert.Null(saved);
    }
}
