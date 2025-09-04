using AutoFixture;
using backend.Data;
using backend.Data.Entities;
using backend.Tests.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Data;

public class ProckDbContextTests
{
    [Theory, AutoMoqData]
    public async Task GetProckConfigAsync_WhenConfigExists_ReturnsConfig(
        IFixture fixture,
        ProckConfig config)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(config);

        // Act
        var result = await context.GetProckConfigAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(config.Id);
        result.UpstreamUrl.Should().Be(config.UpstreamUrl);
    }

    [Theory, AutoMoqData]
    public async Task GetProckConfigAsync_WhenNoConfigExists_ReturnsNull(IFixture fixture)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();

        // Act
        var result = await context.GetProckConfigAsync();

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task GetActiveOpenApiDocumentsAsync_WhenActiveDocumentsExist_ReturnsOnlyActiveDocuments(
        IFixture fixture,
        List<OpenApiSpecification> activeDocuments,
        List<OpenApiSpecification> inactiveDocuments)
    {
        // Arrange
        activeDocuments.ForEach(d => d.IsActive = true);
        inactiveDocuments.ForEach(d => d.IsActive = false);
        
        var allDocuments = activeDocuments.Concat(inactiveDocuments).ToArray();
        await using var context = await TestDbContext.CreateWithEntitiesAsync(allDocuments);

        // Act
        var result = await context.GetActiveOpenApiDocumentsAsync();

        // Assert
        result.Should().HaveCount(activeDocuments.Count);
        result.Should().OnlyContain(d => d.IsActive);
        result.Select(d => d.DocumentId).Should().BeEquivalentTo(activeDocuments.Select(d => d.DocumentId));
    }

    [Theory, AutoMoqData]
    public async Task GetOpenApiDocumentByIdAsync_WhenDocumentExists_ReturnsDocument(
        IFixture fixture,
        OpenApiSpecification document)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(document);

        // Act
        var result = await context.GetOpenApiDocumentByIdAsync(document.DocumentId);

        // Assert
        result.Should().NotBeNull();
        result!.DocumentId.Should().Be(document.DocumentId);
        result.Title.Should().Be(document.Title);
        result.Version.Should().Be(document.Version);
    }

    [Theory, AutoMoqData]
    public async Task GetOpenApiDocumentByIdAsync_WhenDocumentDoesNotExist_ReturnsNull(
        IFixture fixture,
        Guid nonExistentId)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();

        // Act
        var result = await context.GetOpenApiDocumentByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task GetOpenApiDocumentByTitleAsync_WhenActiveDocumentWithTitleExists_ReturnsDocument(
        IFixture fixture,
        OpenApiSpecification document)
    {
        // Arrange
        document.IsActive = true;
        await using var context = await TestDbContext.CreateWithEntitiesAsync(document);

        // Act
        var result = await context.GetOpenApiDocumentByTitleAsync(document.Title!);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(document.Title);
        result.IsActive.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public async Task GetOpenApiDocumentByTitleAsync_WhenDocumentIsInactive_ReturnsNull(
        IFixture fixture,
        OpenApiSpecification document)
    {
        // Arrange
        document.IsActive = false;
        await using var context = await TestDbContext.CreateWithEntitiesAsync(document);

        // Act
        var result = await context.GetOpenApiDocumentByTitleAsync(document.Title!);

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task SaveChangesAsync_WhenAddingMockRoute_SavesSuccessfully(
        IFixture fixture,
        MockRoute mockRoute)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();

        // Act
        context.MockRoutes.Add(mockRoute);
        await context.SaveChangesAsync();

        // Assert
        var savedRoute = await context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == mockRoute.RouteId);
        savedRoute.Should().NotBeNull();
        savedRoute!.Method.Should().Be(mockRoute.Method);
        savedRoute.Path.Should().Be(mockRoute.Path);
        savedRoute.Enabled.Should().Be(mockRoute.Enabled);
    }

    [Theory, AutoMoqData]
    public async Task SaveChangesAsync_WhenUpdatingMockRoute_UpdatesSuccessfully(
        IFixture fixture,
        MockRoute mockRoute)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(mockRoute);
        
        var newPath = "/updated/path";
        var newMethod = "POST";

        // Act
        var routeToUpdate = await context.MockRoutes.FirstAsync(r => r.RouteId == mockRoute.RouteId);
        routeToUpdate.Path = newPath;
        routeToUpdate.Method = newMethod;
        await context.SaveChangesAsync();

        // Assert
        var updatedRoute = await context.MockRoutes.FirstAsync(r => r.RouteId == mockRoute.RouteId);
        updatedRoute.Path.Should().Be(newPath);
        updatedRoute.Method.Should().Be(newMethod);
    }

    [Theory, AutoMoqData]
    public async Task SaveChangesAsync_WhenDeletingMockRoute_RemovesSuccessfully(
        IFixture fixture,
        MockRoute mockRoute)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(mockRoute);

        // Act
        var routeToDelete = await context.MockRoutes.FirstAsync(r => r.RouteId == mockRoute.RouteId);
        context.MockRoutes.Remove(routeToDelete);
        await context.SaveChangesAsync();

        // Assert
        var deletedRoute = await context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == mockRoute.RouteId);
        deletedRoute.Should().BeNull();
    }
}
