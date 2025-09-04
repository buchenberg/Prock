using System;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Data;
using backend.Data.Entities;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


namespace backend.Tests;

public class SimpleEndpointTests
{
    [Fact]
    public void MockRoute_CreatesValidEntity()
    {
        // Arrange
        var mockRoute = new MockRoute
        {
            RouteId = Guid.NewGuid(),
            Method = "GET",
            Path = "/api/test",
            HttpStatusCode = 200,
            Mock = JsonSerializer.Serialize(new { message = "test" }),
            Enabled = true
        };

        // Act & Assert
        mockRoute.RouteId.Should().NotBeEmpty();
        mockRoute.Method.Should().Be("GET");
        mockRoute.Path.Should().Be("/api/test");
        mockRoute.HttpStatusCode.Should().Be(200);
        mockRoute.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task ProckDbContext_CanAddAndRetrieveMockRoute()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProckDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ProckDbContext(options);
        
        var mockRoute = new MockRoute
        {
            RouteId = Guid.NewGuid(),
            Method = "POST",
            Path = "/api/users",
            HttpStatusCode = 201,
            Mock = JsonSerializer.Serialize(new { id = 1, name = "Test User" }),
            Enabled = true
        };

        // Act
        context.MockRoutes.Add(mockRoute);
        await context.SaveChangesAsync();

        // Assert
        var savedRoute = await context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == mockRoute.RouteId);
        savedRoute.Should().NotBeNull();
        savedRoute!.Method.Should().Be("POST");
        savedRoute.Path.Should().Be("/api/users");
        savedRoute.HttpStatusCode.Should().Be(201);
        savedRoute.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public void MockRoute_SupportsAllHttpMethods(string method)
    {
        // Arrange & Act
        var mockRoute = new MockRoute
        {
            RouteId = Guid.NewGuid(),
            Method = method,
            Path = "/api/test",
            HttpStatusCode = 200,
            Mock = "{}",
            Enabled = true
        };

        // Assert
        mockRoute.Method.Should().Be(method);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    public void MockRoute_SupportsValidHttpStatusCodes(int statusCode)
    {
        // Arrange & Act
        var mockRoute = new MockRoute
        {
            RouteId = Guid.NewGuid(),
            Method = "GET",
            Path = "/api/test",
            HttpStatusCode = statusCode,
            Mock = "{}",
            Enabled = true
        };

        // Assert
        mockRoute.HttpStatusCode.Should().Be(statusCode);
    }

    [Fact]
    public void JsonSerialization_WorksCorrectly()
    {
        // Arrange
        var testObject = new { message = "Hello, World!", count = 42, active = true };

        // Act
        var json = JsonSerializer.Serialize(testObject);
        var deserializedElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        json.Should().Contain("Hello, World!");
        json.Should().Contain("42");
        json.Should().Contain("true");
        deserializedElement.ValueKind.Should().Be(JsonValueKind.Object);
        deserializedElement.GetProperty("message").GetString().Should().Be("Hello, World!");
    }
}
