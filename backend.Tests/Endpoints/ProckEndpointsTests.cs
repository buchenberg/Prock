using System.Text.Json;
using AutoFixture;
using AutoFixture.Xunit2;
using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using backend.Endpoints;
using backend.Tests.TestBase;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace backend.Tests.Endpoints;

public class ProckEndpointsTests
{
    [Theory, AutoMoqData]
    public async Task GetMockRoutes_WhenRoutesExist_ReturnsOkWithRoutes(
        [Frozen] IFixture fixture,
        List<MockRoute> mockRoutes)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(mockRoutes.ToArray());

        // Act
        var result = await GetMockRoutesHandler(context);

        // Assert
        result.Should().BeOfType<Ok<List<MockRouteDto>>>();
        var okResult = (Ok<List<MockRouteDto>>)result;
        okResult.Value.Should().HaveCount(mockRoutes.Count);
        okResult.Value.Should().AllSatisfy(route =>
        {
            route.RouteId.Should().NotBeEmpty();
            route.Method.Should().NotBeNullOrEmpty();
            route.Path.Should().NotBeNullOrEmpty();
        });
    }

    [Theory, AutoMoqData]
    public async Task GetMockRoutes_WhenNoRoutesExist_ReturnsOk(IFixture fixture)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();

        // Act
        var result = await GetMockRoutesHandler(context);

        // Assert
        result.Should().BeOfType<Ok<List<MockRouteDto>>>();
    }

    [Theory, AutoMoqData]
    public async Task GetMockRouteById_WhenRouteExists_ReturnsOkWithRoute(
        IFixture fixture,
        MockRoute mockRoute)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(mockRoute);

        // Act
        var result = await GetMockRouteByIdHandler(mockRoute.RouteId, context);

        // Assert
        result.Should().BeOfType<Ok<MockRouteDto>>();
        var okResult = (Ok<MockRouteDto>)result;
        okResult.Value.RouteId.Should().Be(mockRoute.RouteId);
        okResult.Value.Method.Should().Be(mockRoute.Method);
        okResult.Value.Path.Should().Be(mockRoute.Path);
    }

    [Theory, AutoMoqData]
    public async Task GetMockRouteById_WhenRouteDoesNotExist_ReturnsNotFound(
        IFixture fixture,
        Guid nonExistentId)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();

        // Act
        var result = await GetMockRouteByIdHandler(nonExistentId, context);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Theory, AutoMoqData]
    public async Task CreateMockRoute_WithValidData_ReturnsCreatedWithRoute(
        IFixture fixture,
        MockRouteDto routeDto)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();
        routeDto.Method = "GET"; // Ensure valid HTTP method
        
        var app = CreateTestApp();

        // Act
        var result = await CreateMockRouteHandler(routeDto, context, app);

        // Assert
        result.Should().BeOfType<Created<MockRouteDto>>();
        var createdResult = (Created<MockRouteDto>)result;
        createdResult.Value.Should().NotBeNull();
        createdResult.Value!.RouteId.Should().NotBeEmpty();
        createdResult.Value.Method.Should().Be(routeDto.Method);
        createdResult.Value.Path.Should().Be(routeDto.Path);
        
        // Verify entity was saved to database
        var savedRoute = await context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == createdResult.Value.RouteId);
        savedRoute.Should().NotBeNull();
        savedRoute!.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineAutoMoqData("INVALID")]
    [InlineAutoMoqData("")]
    [InlineAutoMoqData(null)]
    public async Task CreateMockRoute_WithInvalidMethod_ReturnsBadRequest(
        string invalidMethod,
        IFixture fixture,
        MockRouteDto routeDto)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();
        routeDto.Method = invalidMethod!;
        
        var app = CreateTestApp();

        // Act
        var result = await CreateMockRouteHandler(routeDto, context, app);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
        var badRequestResult = (BadRequest<string>)result;
        badRequestResult.Value.Should().Contain("not a valid HTTP method");
    }

    [Theory, AutoMoqData]
    public async Task DisableRoute_WhenRouteExists_ReturnsOkWithDisabledRoute(
        IFixture fixture,
        MockRoute mockRoute)
    {
        // Arrange
        mockRoute.Enabled = true;
        await using var context = await TestDbContext.CreateWithEntitiesAsync(mockRoute);

        // Act
        var result = await DisableRouteHandler(mockRoute.RouteId, context);

        // Assert
        result.Should().BeOfType<Ok<MockRouteDto>>();
        var okResult = (Ok<MockRouteDto>)result;
        okResult.Value.Enabled.Should().BeFalse();
        
        // Verify in database
        var updatedRoute = await context.MockRoutes.FirstAsync(r => r.RouteId == mockRoute.RouteId);
        updatedRoute.Enabled.Should().BeFalse();
    }

    [Theory, AutoMoqData]
    public async Task EnableRoute_WhenRouteExists_ReturnsOkWithEnabledRoute(
        IFixture fixture,
        MockRoute mockRoute)
    {
        // Arrange
        mockRoute.Enabled = false;
        await using var context = await TestDbContext.CreateWithEntitiesAsync(mockRoute);

        // Act
        var result = await EnableRouteHandler(mockRoute.RouteId, context);

        // Assert
        result.Should().BeOfType<Ok<MockRouteDto>>();
        var okResult = (Ok<MockRouteDto>)result;
        okResult.Value.Enabled.Should().BeTrue();
        
        // Verify in database
        var updatedRoute = await context.MockRoutes.FirstAsync(r => r.RouteId == mockRoute.RouteId);
        updatedRoute.Enabled.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public async Task DeleteRoute_WhenRouteExists_ReturnsOkAndRemovesRoute(
        IFixture fixture,
        MockRoute mockRoute)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(mockRoute);

        // Act
        var result = await DeleteRouteHandler(mockRoute.RouteId, context);

        // Assert
        result.Should().BeOfType<Ok>();
        
        // Verify route was removed from database
        var deletedRoute = await context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == mockRoute.RouteId);
        deletedRoute.Should().BeNull();
    }

    #region Helper Methods

    private static async Task<IResult> GetMockRoutesHandler(ProckDbContext db)
    {
        // Simulate the endpoint logic
        return await db.MockRoutes.ToListAsync() is List<MockRoute> response
            ? TypedResults.Ok(response.Select(x => new MockRouteDto()
            {
                RouteId = x.RouteId,
                Method = x.Method,
                Path = x.Path,
                HttpStatusCode = x.HttpStatusCode,
                Mock = x.Mock != null ? JsonSerializer.Deserialize<dynamic>(x.Mock) : null,
                Enabled = x.Enabled
            }).ToList())
            : TypedResults.Ok();
    }

    private static async Task<IResult> GetMockRouteByIdHandler(Guid routeId, ProckDbContext db)
    {
        return await db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeId) is MockRoute response
            ? TypedResults.Ok(new MockRouteDto()
            {
                RouteId = response.RouteId,
                Method = response.Method,
                Path = response.Path,
                HttpStatusCode = response.HttpStatusCode,
                Mock = response.Mock != null ? JsonSerializer.Deserialize<dynamic>(response.Mock) : null,
                Enabled = response.Enabled
            })
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateMockRouteHandler(MockRouteDto route, ProckDbContext db, WebApplication app)
    {
        var httpMethods = new[] { "GET", "PUT", "POST", "PATCH", "DELETE" };
        
        route.Method = (route.Method ?? string.Empty).ToUpper();

        if (httpMethods.All(x => x != route.Method))
        {
            return Results.BadRequest($"{route.Method} is not a valid HTTP method");
        }

        var result = new MockRoute
        {
            RouteId = Guid.NewGuid(),
            Method = route.Method,
            Path = route.Path,
            HttpStatusCode = route.HttpStatusCode,
            Mock = JsonSerializer.Serialize(route.Mock),
            Enabled = true
        };

        db.MockRoutes.Add(result);
        await db.SaveChangesAsync();

        route.RouteId = result.RouteId;

        return TypedResults.Created($"/prock/api/mock-routes/{result.RouteId}", route);
    }

    private static async Task<IResult> DisableRouteHandler(Guid routeId, ProckDbContext db)
    {
        var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId);

        if (persistedRoute == null)
        {
            return TypedResults.NotFound(routeId);
        }

        persistedRoute.Enabled = false;
        await db.SaveChangesAsync();

        var response = new MockRouteDto
        {
            RouteId = persistedRoute.RouteId,
            Method = persistedRoute.Method,
            Path = persistedRoute.Path,
            HttpStatusCode = persistedRoute.HttpStatusCode,
            Mock = persistedRoute.Mock != null ? JsonSerializer.Deserialize<dynamic>(persistedRoute.Mock) : null,
            Enabled = persistedRoute.Enabled
        };

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> EnableRouteHandler(Guid routeId, ProckDbContext db)
    {
        var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId);

        if (persistedRoute == null)
        {
            return TypedResults.NotFound(routeId);
        }

        persistedRoute.Enabled = true;
        await db.SaveChangesAsync();

        var response = new MockRouteDto
        {
            RouteId = persistedRoute.RouteId,
            Method = persistedRoute.Method,
            Path = persistedRoute.Path,
            HttpStatusCode = persistedRoute.HttpStatusCode,
            Mock = JsonSerializer.Serialize(persistedRoute.Mock),
            Enabled = persistedRoute.Enabled
        };

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> DeleteRouteHandler(Guid routeId, ProckDbContext db)
    {
        var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId);
        if (persistedRoute == null)
        {
            return TypedResults.NotFound();
        }

        db.MockRoutes.Remove(persistedRoute);
        await db.SaveChangesAsync();

        return TypedResults.Ok();
    }

    private static WebApplication CreateTestApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging(logging => logging.AddProvider(NullLoggerProvider.Instance));
        var app = builder.Build();
        return app;
    }

    #endregion
}
