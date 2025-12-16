using backend.Data;
using backend.Data.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Readers;
using OpenApiSpecification = backend.Data.Entities.OpenApiSpecification;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MsOpenApi = Microsoft.OpenApi.Models;
using backend.Data.Entities;
using Microsoft.OpenApi.Any;
using backend.Services;
using backend.Utils;

namespace backend.Endpoints;

public static class OpenApiEndpoints
{

    public static void RegisterOpenApiEndpoints(this WebApplication app)
    {
        // Get all OpenAPI documents
        app.MapGet("/prock/api/openapi-documents", async (IOpenApiService service) => 
            await service.GetAllDocumentsAsync());

        // Get specific OpenAPI document by ID
        app.MapGet("/prock/api/openapi-documents/{documentId}", async (Guid documentId, IOpenApiService service) => 
            await service.GetDocumentByIdAsync(documentId));

        // Create new OpenAPI document
        app.MapPost("/prock/api/openapi-documents", async (CreateOpenApiDocumentDto request, IOpenApiService service) => 
            await service.CreateDocumentAsync(request));

        // Update OpenAPI document
        app.MapPut("/prock/api/openapi-documents/{documentId}", async (Guid documentId, UpdateOpenApiDocumentDto request, IOpenApiService service) => 
            await service.UpdateDocumentAsync(documentId, request));

        // Delete OpenAPI document
        app.MapDelete("/prock/api/openapi-documents/{documentId}", async (Guid documentId, IOpenApiService service) => 
            await service.DeleteDocumentAsync(documentId));

        // Get OpenAPI JSON for a specific document
        app.MapGet("/prock/api/openapi-documents/{documentId}/json", async Task<IResult> (Guid documentId, IOpenApiService service) =>
        {
            var result = await service.GetDocumentByIdAsync(documentId);
            if (result.Result is Ok<OpenApiDocumentDetailDto> ok && ok.Value != null && !string.IsNullOrEmpty(ok.Value.OriginalJson))
            {
                var parsed = MockDataGenerator.ParseOpenApiJson(ok.Value.OriginalJson);
                if (parsed != null)
                {
                    return TypedResults.Ok(parsed);
                }
                return TypedResults.BadRequest("Invalid OpenAPI JSON format");
            }
            return TypedResults.NotFound();
        });

        // Generate mock routes
        app.MapPost("/prock/api/openapi-documents/{documentId}/generate-mock-routes", async (Guid documentId, IOpenApiService service) => 
            await service.GenerateMockRoutesAsync(documentId));
    }
}
