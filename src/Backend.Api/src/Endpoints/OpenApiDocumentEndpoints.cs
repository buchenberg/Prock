using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Contracts.Models;
using Shared.Contracts.Requests;
using Shared.Contracts.Responses;

namespace Backend.Api.Endpoints;

public static class OpenApiDocumentEndpoints
{
    public static void RegisterOpenApiDocumentEndpoints(this WebApplication app)
    {
        var routes = app.MapGroup(ApiRoutes.OpenApi.Base)
            .WithTags("OpenAPI Documents")
            .WithOpenApi();

        // GET /prock/api/openapi/documents
        routes.MapGet("documents", GetAllDocuments)
            .WithName("GetAllOpenApiDocuments")
            .WithSummary("Get all active OpenAPI documents")
            .Produces<IEnumerable<OpenApiDocumentResponse>>();

        // GET /prock/api/openapi/documents/{id}
        routes.MapGet("documents/{id}", GetDocumentById)
            .WithName("GetOpenApiDocumentById")
            .WithSummary("Get an OpenAPI document by ID")
            .Produces<OpenApiDocumentResponse>()
            .Produces(404);

        // POST /prock/api/openapi/documents
        routes.MapPost("documents", CreateDocument)
            .WithName("CreateOpenApiDocument")
            .WithSummary("Create a new OpenAPI document")
            .Produces<OpenApiDocumentResponse>(201)
            .ProducesValidationProblem();

        // PUT /prock/api/openapi/documents/{id}
        routes.MapPut("documents/{id}", UpdateDocument)
            .WithName("UpdateOpenApiDocument")
            .WithSummary("Update an existing OpenAPI document")
            .Produces<OpenApiDocumentResponse>()
            .Produces(404)
            .ProducesValidationProblem();

        // DELETE /prock/api/openapi/documents/{id}
        routes.MapDelete("documents/{id}", DeleteDocument)
            .WithName("DeleteOpenApiDocument")
            .WithSummary("Delete an OpenAPI document")
            .Produces(204)
            .Produces(404);

        // POST /prock/api/openapi/documents/{id}/generate-mocks
        routes.MapPost("documents/{id}/generate-mocks", GenerateMocksFromDocument)
            .WithName("GenerateMocksFromOpenApiDocument")
            .WithSummary("Generate mock routes from an OpenAPI document")
            .Produces<IEnumerable<MockRouteResponse>>(201)
            .Produces(404);
    }

    private static async Task<Results<Ok<IEnumerable<OpenApiDocumentResponse>>, ProblemHttpResult>> GetAllDocuments(
        IOpenApiService openApiService)
    {
        try
        {
            var documents = await openApiService.GetActiveDocumentsAsync();
            var responses = documents.Select(MapToResponse);
            return TypedResults.Ok(responses);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve OpenAPI documents: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<OpenApiDocumentResponse>, NotFound, ProblemHttpResult>> GetDocumentById(
        string id, IOpenApiService openApiService)
    {
        try
        {
            var document = await openApiService.GetDocumentByIdAsync(id);
            return document != null
                ? TypedResults.Ok(MapToResponse(document))
                : TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Created<OpenApiDocumentResponse>, ValidationProblem, ProblemHttpResult>> CreateDocument(
        CreateOpenApiDocumentRequest request, IOpenApiService openApiService, ILogger<Program> logger)
    {
        try
        {
            var document = MapToEntity(request);
            var created = await openApiService.CreateDocumentAsync(document);

            logger.LogInformation("Created OpenAPI document {DocumentId} with title '{Title}'",
                created.DocumentId, created.Title);

            var response = MapToResponse(created);
            return TypedResults.Created($"/prock/api/openapi/documents/{created.DocumentId}", response);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to create OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<OpenApiDocumentResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateDocument(
        string id, CreateOpenApiDocumentRequest request, IOpenApiService openApiService, ILogger<Program> logger)
    {
        try
        {
            var document = MapToEntity(request);
            var updated = await openApiService.UpdateDocumentAsync(id, document);

            logger.LogInformation("Updated OpenAPI document {DocumentId} with title '{Title}'",
                updated.DocumentId, updated.Title);

            return TypedResults.Ok(MapToResponse(updated));
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to update OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteDocument(
        string id, IOpenApiService openApiService, ILogger<Program> logger)
    {
        try
        {
            var deleted = await openApiService.DeleteDocumentAsync(id);
            if (!deleted)
            {
                return TypedResults.NotFound();
            }

            logger.LogInformation("Deleted OpenAPI document {DocumentId}", id);
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to delete OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Created<IEnumerable<MockRouteResponse>>, NotFound, ProblemHttpResult>> GenerateMocksFromDocument(
        string id, IOpenApiService openApiService, IMockRouteService mockRouteService, ILogger<Program> logger)
    {
        try
        {
            var document = await openApiService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return TypedResults.NotFound();
            }

            // TODO: Implement mock generation logic
            // This would parse the OpenAPI document and create mock routes
            var mockRoutes = new List<MockRouteResponse>();

            logger.LogInformation("Generated {Count} mock routes from OpenAPI document {DocumentId}",
                mockRoutes.Count, id);

            return TypedResults.Created($"/prock/api/mock-routes", mockRoutes.AsEnumerable());
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to generate mocks from OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    // Mapping helpers
    private static OpenApiDocumentResponse MapToResponse(Backend.Core.Domain.Entities.OpenApi.OpenApiSpecification entity)
    {
        return new OpenApiDocumentResponse
        {
            DocumentId = entity.DocumentId.ToString(),
            Title = entity.Title ?? string.Empty,
            Version = entity.Version,
            Description = entity.Description,
            OpenApiVersion = entity.OpenApiVersion,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive,
            PathCount = entity.Paths?.Count ?? 0
        };
    }

    private static Backend.Core.Domain.Entities.OpenApi.OpenApiSpecification MapToEntity(CreateOpenApiDocumentRequest request)
    {
        return new Backend.Core.Domain.Entities.OpenApi.OpenApiSpecification
        {
            DocumentId = Guid.NewGuid(),
            Title = request.Title,
            Version = request.Version,
            Description = request.Description,
            OriginalJson = request.OriginalJson,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
