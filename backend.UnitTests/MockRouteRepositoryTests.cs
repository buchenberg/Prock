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

    #region IsPathMatch Tests

    [Theory]
    [InlineData("/api/v1.0/projects/{projectKey}/monitoring/samples", "/api/v1.0/projects/ROI-059581/monitoring/samples", true)]
    [InlineData("/api/v1/projects/{projectKey}/jobs/issue-rewards/{jobKey}/report", "/api/v1/projects/someprojectkey/jobs/issue-rewards/somejobkey/report", true)]
    [InlineData("/api/users/{userId}", "/api/users/123", true)]
    [InlineData("/api/users/{userId}", "/api/users/abc-def-ghi", true)]
    [InlineData("/api/users/{userId}/posts/{postId}", "/api/users/123/posts/456", true)]
    [InlineData("/api/users", "/api/users", true)]  // No path params - exact match
    [InlineData("/api/users/{userId}", "/api/users/123/extra", false)]  // Extra path segment
    [InlineData("/api/users/{userId}", "/api/users", false)]  // Missing path param
    [InlineData("/api/users/{userId}/posts", "/api/users/123/comments", false)]  // Wrong suffix
    [InlineData("/api/{version}/users/{userId}", "/api/v1/users/123", true)]  // Multiple path params
    public void IsPathMatch_ReturnsExpectedResult(string routeTemplate, string requestPath, bool expected)
    {
        var result = MockRouteRepository.IsPathMatch(routeTemplate, requestPath);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task FindMatchingRouteAsync_MatchesWildcardPath()
    {
        // Arrange - Create a route with path parameters
        var route = new MockRoute 
        { 
            _id = ObjectId.GenerateNewId(), 
            RouteId = Guid.NewGuid(), 
            Path = "/api/v1.0/projects/{projectKey}/monitoring/samples", 
            Method = "GET",
            Mock = "{}",
            Enabled = true 
        };
        _context.MockRoutes.Add(route);
        await _context.SaveChangesAsync();

        // Act - Try to match with actual path values
        var result = await _repository.FindMatchingRouteAsync(
            "/api/v1.0/projects/ROI-059581/monitoring/samples", 
            "GET");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(route.RouteId, result.RouteId);
    }

    [Fact]
    public async Task FindMatchingRouteAsync_MatchesMultipleWildcards()
    {
        // Arrange - Create a route with multiple path parameters
        var route = new MockRoute 
        { 
            _id = ObjectId.GenerateNewId(), 
            RouteId = Guid.NewGuid(), 
            Path = "/api/v1/projects/{projectKey}/jobs/issue-rewards/{jobKey}/report", 
            Method = "GET",
            Mock = "{}",
            Enabled = true 
        };
        _context.MockRoutes.Add(route);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindMatchingRouteAsync(
            "/api/v1/projects/myproject/jobs/issue-rewards/myjob123/report", 
            "GET");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(route.RouteId, result.RouteId);
    }

    [Fact]
    public async Task FindMatchingRouteAsync_PrefersExactMatch()
    {
        // Arrange - Create both an exact match and a pattern match
        var wildcardRoute = new MockRoute 
        { 
            _id = ObjectId.GenerateNewId(), 
            RouteId = Guid.NewGuid(), 
            Path = "/api/users/{userId}", 
            Method = "GET",
            Mock = "{\"type\":\"wildcard\"}",
            Enabled = true 
        };
        var exactRoute = new MockRoute 
        { 
            _id = ObjectId.GenerateNewId(), 
            RouteId = Guid.NewGuid(), 
            Path = "/api/users/special", 
            Method = "GET",
            Mock = "{\"type\":\"exact\"}",
            Enabled = true 
        };
        _context.MockRoutes.AddRange(wildcardRoute, exactRoute);
        await _context.SaveChangesAsync();

        // Act - Request the exact path
        var result = await _repository.FindMatchingRouteAsync("/api/users/special", "GET");

        // Assert - Should prefer exact match
        Assert.NotNull(result);
        Assert.Equal(exactRoute.RouteId, result.RouteId);
    }

    [Fact]
    public async Task FindMatchingRouteAsync_DoesNotMatchDisabledRoutes()
    {
        // Arrange
        var route = new MockRoute 
        { 
            _id = ObjectId.GenerateNewId(), 
            RouteId = Guid.NewGuid(), 
            Path = "/api/users/{userId}", 
            Method = "GET",
            Mock = "{}",
            Enabled = false  // Disabled!
        };
        _context.MockRoutes.Add(route);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindMatchingRouteAsync("/api/users/123", "GET");

        // Assert
        Assert.Null(result);
    }

    #endregion
}

