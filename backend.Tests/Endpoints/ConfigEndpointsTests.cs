using System.Text.Json;
using AutoFixture;
using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
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
        config.GetProperty("connectionString").GetString().Should().Be("mongodb://test:27017/");
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
        var updatedConfig = await context.ProckConfig.FirstAsync();
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
        okResult.Value.Id.Should().NotBeEmpty();

        // Verify new config was created in database
        var configs = await context.ProckConfig.ToListAsync();
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

    private static async Task<IResult> GetConfigHandler(ProckDbContext db, IConfiguration configuration)
    {
        var host = configuration.GetSection("Prock").GetSection("Host").Value ?? "http://localhost";
        var port = configuration.GetSection("Prock").GetSection("Port").Value ?? "5001";
        var connectionString = configuration.GetSection("Prock").GetSection("MongoDbUri").Value ?? "mongodb://localhost:27017/";
        
        var config = await db.GetProckConfigAsync();
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

    private static async Task<IResult> UpdateUpstreamUrlHandler(ProckConfigDto update, ProckDbContext db)
    {
        var config = await db.ProckConfig.SingleOrDefaultAsync();
        if (config == null)
        {
            config = new ProckConfig()
            {
                Id = Guid.NewGuid(),
                UpstreamUrl = update.UpstreamUrl
            };
            db.ProckConfig.Add(config);
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
            ["Prock:MongoDbUri"] = "mongodb://test:27017/",
            ["Prock:DbName"] = "test-db"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    #endregion
}
