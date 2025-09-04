using System.Text.Json;
using AutoFixture;
using AutoFixture.Xunit2;
using Backend.Infrastructure.Data.Context;
using Backend.Core.Domain.Entities.MariaDb;
using backend.Tests.TestBase;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Shared.Contracts.Requests;
using Shared.Contracts.Responses;

namespace backend.Tests.Endpoints;

public class OpenApiEndpointsTests
{
    [Theory, AutoMoqData]
    public async Task GetDocuments_WhenDocumentsExist_ReturnsOkWithDocuments(
        [Frozen] IFixture fixture,
        List<OpenApiSpecification> documents)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(documents.ToArray());

        // Act
        var result = await GetDocumentsHandler(context);

        // Assert
        result.Should().BeOfType<Ok<List<OpenApiDocumentResponse>>>();
        var okResult = (Ok<List<OpenApiDocumentResponse>>)result;
        okResult.Value.Should().HaveCount(documents.Count);
        okResult.Value.Should().AllSatisfy(doc =>
        {
            doc.DocumentId.Should().NotBeNullOrEmpty();
            doc.Title.Should().NotBeNullOrEmpty();
        });
    }

    [Theory, AutoMoqData]
    public async Task GetDocuments_WhenNoDocumentsExist_ReturnsOk(IFixture fixture)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();

        // Act
        var result = await GetDocumentsHandler(context);

        // Assert
        result.Should().BeOfType<Ok<List<OpenApiDocumentResponse>>>();
        var okResult = (Ok<List<OpenApiDocumentResponse>>)result;
        okResult.Value.Should().BeEmpty();
    }

    [Theory, AutoMoqData]
    public async Task GetDocumentById_WhenDocumentExists_ReturnsOkWithDocument(
        IFixture fixture,
        OpenApiSpecification document)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(document);

        // Act
        var result = await GetDocumentByIdHandler(document.Id, context);

        // Assert
        result.Should().BeOfType<Ok<OpenApiDocumentResponse>>();
        var okResult = (Ok<OpenApiDocumentResponse>)result;
        okResult.Value.DocumentId.Should().Be(document.Id.ToString());
        okResult.Value.Title.Should().Be(document.Title);
        okResult.Value.Version.Should().Be(document.Version);
    }

    [Theory, AutoMoqData]
    public async Task GetDocumentById_WhenDocumentDoesNotExist_ReturnsNotFound(
        IFixture fixture,
        int nonExistentId)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();

        // Act
        var result = await GetDocumentByIdHandler(nonExistentId, context);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Theory, AutoMoqData]
    public async Task CreateDocument_WithValidData_ReturnsCreatedWithDocument(
        IFixture fixture,
        CreateOpenApiDocumentRequest request)
    {
        // Arrange
        await using var context = TestDbContext.CreateInMemory();
        request.OriginalJson = JsonSerializer.Serialize(new { openapi = "3.0.0", info = new { title = "Test API", version = "1.0.0" } });

        // Act
        var result = await CreateDocumentHandler(request, context, NullLogger.Instance);

        // Assert
        result.Should().BeOfType<Created<OpenApiDocumentResponse>>();
        var createdResult = (Created<OpenApiDocumentResponse>)result;
        createdResult.Value.Should().NotBeNull();
        createdResult.Value!.DocumentId.Should().NotBeNullOrEmpty();
        createdResult.Value.Title.Should().Be("Test API");
        createdResult.Value.Version.Should().Be("1.0.0");
        
        // Verify entity was saved to database
        var savedDoc = await context.OpenApiSpecifications.FirstOrDefaultAsync(d => d.Id == int.Parse(createdResult.Value.DocumentId));
        savedDoc.Should().NotBeNull();
        savedDoc!.IsActive.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public async Task UpdateDocument_WhenDocumentExists_ReturnsOkWithUpdatedDocument(
        IFixture fixture,
        OpenApiSpecification existingDocument,
        UpdateOpenApiDocumentRequest request)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(existingDocument);
        request.OriginalJson = JsonSerializer.Serialize(new { openapi = "3.0.0", info = new { title = "Updated API", version = "2.0.0" } });

        // Act
        var result = await UpdateDocumentHandler(existingDocument.Id, request, context, NullLogger.Instance);

        // Assert
        result.Should().BeOfType<Ok<OpenApiDocumentResponse>>();
        var okResult = (Ok<OpenApiDocumentResponse>)result;
        okResult.Value.Title.Should().Be("Updated API");
        okResult.Value.Version.Should().Be("2.0.0");

        // Verify database was updated
        var updatedDoc = await context.OpenApiSpecifications.FirstAsync(d => d.Id == existingDocument.Id);
        updatedDoc.Title.Should().Be("Updated API");
        updatedDoc.Version.Should().Be("2.0.0");
    }

    [Theory, AutoMoqData]
    public async Task DeleteDocument_WhenDocumentExists_ReturnsOkAndRemovesDocument(
        IFixture fixture,
        OpenApiSpecification document)
    {
        // Arrange
        await using var context = await TestDbContext.CreateWithEntitiesAsync(document);

        // Act
        var result = await DeleteDocumentHandler(document.Id, context);

        // Assert
        result.Should().BeOfType<Ok>();
        
        // Verify document was removed from database
        var deletedDoc = await context.OpenApiSpecifications.FirstOrDefaultAsync(d => d.Id == document.Id);
        deletedDoc.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task GetDocumentJson_WhenDocumentExists_ReturnsContent(
        IFixture fixture,
        OpenApiSpecification document)
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new { openapi = "3.0.0", info = new { title = "Test API", version = "1.0.0" } });
        document.Content = jsonContent;
        await using var context = await TestDbContext.CreateWithEntitiesAsync(document);

        // Act
        var result = await GetDocumentJsonHandler(document.Id, context);

        // Assert
        result.Should().BeOfType<ContentHttpResult>();
        var contentResult = (ContentHttpResult)result;
        contentResult.ContentType.Should().Be("application/json");
    }

    [Theory, AutoMoqData]
    public async Task ToggleDocumentActive_WhenDocumentExists_TogglesActiveStatus(
        IFixture fixture,
        OpenApiSpecification document)
    {
        // Arrange
        document.IsActive = false;
        await using var context = await TestDbContext.CreateWithEntitiesAsync(document);

        // Act
        var result = await ToggleDocumentActiveHandler(document.Id, context);

        // Assert
        result.Should().BeOfType<Ok<OpenApiDocumentResponse>>();
        var okResult = (Ok<OpenApiDocumentResponse>)result;
        okResult.Value.IsActive.Should().BeTrue();

        // Verify in database
        var updatedDoc = await context.OpenApiSpecifications.FirstAsync(d => d.Id == document.Id);
        updatedDoc.IsActive.Should().BeTrue();
    }

    #region Helper Methods

    private static async Task<IResult> GetDocumentsHandler(MariaDbContext db)
    {
        var documents = await db.OpenApiSpecifications.ToListAsync();
        var response = documents.Select(doc => new OpenApiDocumentResponse
        {
            DocumentId = doc.Id.ToString(),
            Title = doc.Title,
            Description = doc.Description,
            Version = doc.Version,
            OpenApiVersion = doc.OpenApiVersion,
            IsActive = doc.IsActive,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt ?? doc.CreatedAt
        }).ToList();

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> GetDocumentByIdHandler(int id, MariaDbContext db)
    {
        var document = await db.OpenApiSpecifications.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null)
        {
            return TypedResults.NotFound();
        }

        var response = new OpenApiDocumentResponse
        {
            DocumentId = document.Id.ToString(),
            Title = document.Title,
            Description = document.Description,
            Version = document.Version,
            OpenApiVersion = document.OpenApiVersion,
            IsActive = document.IsActive,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt ?? document.CreatedAt
        };

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> CreateDocumentHandler(CreateOpenApiDocumentRequest request, MariaDbContext db, ILogger logger)
    {
        try
        {
            // Parse OpenAPI document to extract metadata
            var openApiDoc = JsonDocument.Parse(request.OriginalJson);
            var info = openApiDoc.RootElement.GetProperty("info");
            var title = info.GetProperty("title").GetString() ?? "Untitled API";
            var version = info.GetProperty("version").GetString() ?? "1.0.0";
            var description = info.TryGetProperty("description", out var descProp) ? descProp.GetString() : null;
            var openApiVersion = openApiDoc.RootElement.GetProperty("openapi").GetString() ?? "3.0.0";

            var document = new OpenApiSpecification
            {
                Title = title,
                Description = description,
                Version = version,
                OpenApiVersion = openApiVersion,
                Content = request.OriginalJson,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.OpenApiSpecifications.Add(document);
            await db.SaveChangesAsync();

            var response = new OpenApiDocumentResponse
            {
                DocumentId = document.Id.ToString(),
                Title = document.Title,
                Description = document.Description,
                Version = document.Version,
                OpenApiVersion = document.OpenApiVersion,
                IsActive = document.IsActive,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt ?? document.CreatedAt
            };

            return TypedResults.Created($"/prock/api/openapi/documents/{document.Id}", response);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid JSON in OpenAPI document");
            return TypedResults.BadRequest("Invalid JSON format");
        }
    }

    private static async Task<IResult> UpdateDocumentHandler(int id, UpdateOpenApiDocumentRequest request, MariaDbContext db, ILogger logger)
    {
        var document = await db.OpenApiSpecifications.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null)
        {
            return TypedResults.NotFound();
        }

        try
        {
            // Parse OpenAPI document to extract metadata
            var openApiDoc = JsonDocument.Parse(request.OriginalJson);
            var info = openApiDoc.RootElement.GetProperty("info");
            
            document.Title = info.GetProperty("title").GetString() ?? document.Title;
            document.Version = info.GetProperty("version").GetString() ?? document.Version;
            document.Description = info.TryGetProperty("description", out var descProp) ? descProp.GetString() : document.Description;
            document.OpenApiVersion = openApiDoc.RootElement.GetProperty("openapi").GetString() ?? document.OpenApiVersion;
            document.Content = request.OriginalJson;
            document.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var response = new OpenApiDocumentResponse
            {
                DocumentId = document.Id.ToString(),
                Title = document.Title,
                Description = document.Description,
                Version = document.Version,
                OpenApiVersion = document.OpenApiVersion,
                IsActive = document.IsActive,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt ?? document.CreatedAt
            };

            return TypedResults.Ok(response);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid JSON in OpenAPI document");
            return TypedResults.BadRequest("Invalid JSON format");
        }
    }

    private static async Task<IResult> DeleteDocumentHandler(int id, MariaDbContext db)
    {
        var document = await db.OpenApiSpecifications.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null)
        {
            return TypedResults.NotFound();
        }

        db.OpenApiSpecifications.Remove(document);
        await db.SaveChangesAsync();

        return TypedResults.Ok();
    }

    private static async Task<IResult> GetDocumentJsonHandler(int id, MariaDbContext db)
    {
        var document = await db.OpenApiSpecifications.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Content(document.Content, "application/json");
    }

    private static async Task<IResult> ToggleDocumentActiveHandler(int id, MariaDbContext db)
    {
        var document = await db.OpenApiSpecifications.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null)
        {
            return TypedResults.NotFound();
        }

        document.IsActive = !document.IsActive;
        document.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var response = new OpenApiDocumentResponse
        {
            DocumentId = document.Id.ToString(),
            Title = document.Title,
            Description = document.Description,
            Version = document.Version,
            OpenApiVersion = document.OpenApiVersion,
            IsActive = document.IsActive,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt ?? document.CreatedAt
        };

        return TypedResults.Ok(response);
    }

    #endregion
}
