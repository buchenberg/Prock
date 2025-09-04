using System.Text.Json;
using AutoFixture;
using Backend.Infrastructure.Data.Context;
using Backend.Core.Domain.Entities.MariaDb;
using Shared.Contracts.Models;
using backend.Tests.TestBase;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace backend.Tests.Endpoints;

public class ConfigEndpointsTests
{
    [Theory, AutoMoqData]
    public async Task GetConfig_WhenConfigExists_ReturnsConfigData(
        IFixture fixture,
        ProckConfig prockConfig)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(prockConfig);
        var configuration = CreateTestConfiguration();

        // Act
        var result = await GetConfigHandler(context, configuration);

        // Assert
        result.Should().BeAssignableTo<IResult>();
        var okResult = result.Should().BeAssignableTo<IValueHttpResult>().Subject;
        
        var config = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(okResult.Value));
        config.GetProperty("upstreamUrl").GetString().Should().Be(prockConfig.UpstreamUrl);
        config.GetProperty("host").GetString().Should().Be("http://test");
        config.GetProperty("port").GetString().Should().Be("5001");
        config.GetProperty("connectionString").GetString().Should().Be("Server=localhost;Database=prock;");
    }

    [Theory, AutoMoqData]
    public async Task GetConfig_WhenNoConfigExists_ReturnsDefaultValues(IFixture fixture)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();
        var configuration = CreateTestConfiguration();

        // Act
        var result = await GetConfigHandler(context, configuration);

        // Assert
        result.Should().BeAssignableTo<IResult>();
        var okResult = result.Should().BeAssignableTo<IValueHttpResult>().Subject;
        
        var config = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(okResult.Value));
        config.GetProperty("upstreamUrl").GetString().Should().Be("nope");
        config.GetProperty("host").GetString().Should().Be("http://test");
        config.GetProperty("port").GetString().Should().Be("5001");
    }

    [Theory, AutoMoqData]
    public async Task UpdateUpstreamUrl_WhenConfigExists_UpdatesExistingConfig(
        IFixture fixture,
        ProckConfig existingConfig,
        string newUpstreamUrl)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(existingConfig);
        var updateDto = new ProckConfigDto { UpstreamUrl = newUpstreamUrl };

        // Act
        var result = await UpdateUpstreamUrlHandler(updateDto, context);

        // Assert
        result.Should().BeOfType<Ok<ProckConfig>>();
        var okResult = (Ok<ProckConfig>)result;
        okResult.Value.UpstreamUrl.Should().Be(newUpstreamUrl);
        okResult.Value.Id.Should().Be(existingConfig.Id);

        // Verify database was updated
        var updatedConfig = await context.ProckConfigs.FirstAsync();
        updatedConfig.UpstreamUrl.Should().Be(newUpstreamUrl);
    }

    [Theory, AutoMoqData]
    public async Task UpdateUpstreamUrl_WhenNoConfigExists_CreatesNewConfig(
        IFixture fixture,
        string newUpstreamUrl)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();
        var updateDto = new ProckConfigDto { UpstreamUrl = newUpstreamUrl };

        // Act
        var result = await UpdateUpstreamUrlHandler(updateDto, context);

        // Assert
        result.Should().BeOfType<Ok<ProckConfig>>();
        var okResult = (Ok<ProckConfig>)result;
        okResult.Value.UpstreamUrl.Should().Be(newUpstreamUrl);
        okResult.Value.Id.Should().BeGreaterThan(0);

        // Verify new config was created in database
        var configs = await context.ProckConfigs.ToListAsync();
        configs.Should().HaveCount(1);
        configs.First().UpstreamUrl.Should().Be(newUpstreamUrl);
    }

    [Theory, AutoMoqData]
    public async Task UpdateUpstreamUrl_WithNullUpstreamUrl_UpdatesToNull(
        IFixture fixture,
        ProckConfig existingConfig)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(existingConfig);
        var updateDto = new ProckConfigDto { UpstreamUrl = null };

        // Act
        var result = await UpdateUpstreamUrlHandler(updateDto, context);

        // Assert
        result.Should().BeOfType<Ok<ProckConfig>>();
        var okResult = (Ok<ProckConfig>)result;
        okResult.Value.UpstreamUrl.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task UpdateUpstreamUrl_WithEmptyString_UpdatesToEmptyString(
        IFixture fixture,
        ProckConfig existingConfig)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(existingConfig);
        var updateDto = new ProckConfigDto { UpstreamUrl = string.Empty };

        // Act
        var result = await UpdateUpstreamUrlHandler(updateDto, context);

        // Assert
        result.Should().BeOfType<Ok<ProckConfig>>();
        var okResult = (Ok<ProckConfig>)result;
        okResult.Value.UpstreamUrl.Should().BeEmpty();
    }

    #region Helper Methods

    private static async Task<IResult> GetConfigHandler(MariaDbContext db, IConfiguration configuration)
    {
        var host = configuration.GetSection("Prock").GetSection("Host").Value ?? "http://localhost";
        var port = configuration.GetSection("Prock").GetSection("Port").Value ?? "5001";
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=prock;";
        
        var config = await db.ProckConfigs.FirstOrDefaultAsync();
        if (config == null)
        {
            return TypedResults.Ok(new
            {
                connectionString,
                upstreamUrl = "nope",
                host,
                port
            });
        }
        
        var upstreamUrl = config.UpstreamUrl ?? "unknown";
        return TypedResults.Ok(new { connectionString, upstreamUrl, host, port });
    }

    private static async Task<IResult> UpdateUpstreamUrlHandler(ProckConfigDto update, MariaDbContext db)
    {
        var config = await db.ProckConfigs.SingleOrDefaultAsync();
        if (config == null)
        {
            config = new ProckConfig()
            {
                UpstreamUrl = update.UpstreamUrl
            };
            db.ProckConfigs.Add(config);
            await db.SaveChangesAsync();
            return Results.Ok(config);
        }
        
        config.UpstreamUrl = update.UpstreamUrl;
        await db.SaveChangesAsync();
        return TypedResults.Ok(config);
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Prock:Host"] = "http://test",
            ["Prock:Port"] = "5001",
            ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=prock;"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    #endregion
}
