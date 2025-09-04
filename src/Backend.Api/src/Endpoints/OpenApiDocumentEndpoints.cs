using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Shared.Contracts.Models;
using Shared.Contracts.Requests;
using Shared.Contracts.Responses;
using System.Text.Json;

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
        routes.MapGet("documents/{id:int}", GetDocumentById)
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
        routes.MapPut("documents/{id:int}", UpdateDocument)
            .WithName("UpdateOpenApiDocument")
            .WithSummary("Update an existing OpenAPI document")
            .Produces<OpenApiDocumentResponse>()
            .Produces(404)
            .ProducesValidationProblem();

        // DELETE /prock/api/openapi/documents/{id}
        routes.MapDelete("documents/{id:int}", DeleteDocument)
            .WithName("DeleteOpenApiDocument")
            .WithSummary("Delete an OpenAPI document")
            .Produces(204)
            .Produces(404);

        // GET /prock/api/openapi/documents/{id}/json
        routes.MapGet("documents/{id:int}/json", GetDocumentJson)
            .WithName("GetOpenApiDocumentJson")
            .WithSummary("Get the JSON content of an OpenAPI document")
            .Produces<object>(200)
            .Produces(404);

        // POST /prock/api/openapi/documents/{id}/generate-mocks
        routes.MapPost("documents/{id:int}/generate-mocks", GenerateMocksFromDocument)
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
            var documents = await openApiService.GetActiveAsync();
            var responses = documents.Select(MapToResponse);
            return TypedResults.Ok(responses);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve OpenAPI documents: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<OpenApiDocumentResponse>, NotFound, ProblemHttpResult>> GetDocumentById(
        int id, IOpenApiService openApiService)
    {
        try
        {
            var document = await openApiService.GetByIdAsync(id);
            return document != null
                ? TypedResults.Ok(MapToResponse(document))
                : TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<object>, NotFound, ProblemHttpResult>> GetDocumentJson(
        int id, IOpenApiService openApiService)
    {
        try
        {
            var document = await openApiService.GetByIdAsync(id);
            if (document == null)
                return TypedResults.NotFound();

            // Parse the JSON content and return it as an object
            var jsonContent = System.Text.Json.JsonSerializer.Deserialize<object>(document.Content);
            return TypedResults.Ok(jsonContent);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return TypedResults.Problem($"Invalid JSON content in document: {ex.Message}", statusCode: 500);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve OpenAPI document JSON: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Created<OpenApiDocumentResponse>, ValidationProblem, ProblemHttpResult>> CreateDocument(
        CreateOpenApiDocumentRequest request, IOpenApiService openApiService, ILogger<Program> logger)
    {
        try
        {
            var document = MapToEntity(request);
            var created = await openApiService.CreateAsync(document);

            logger.LogInformation("Created OpenAPI document {DocumentId} with title '{Title}'",
                created.Id, created.Title);

            var response = MapToResponse(created);
            return TypedResults.Created($"/prock/api/openapi/documents/{created.Id}", response);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to create OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<OpenApiDocumentResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateDocument(
        int id, CreateOpenApiDocumentRequest request, IOpenApiService openApiService, ILogger<Program> logger)
    {
        try
        {
            var document = MapToEntity(request);
            var updated = await openApiService.UpdateAsync(id, document);

            logger.LogInformation("Updated OpenAPI document {DocumentId} with title '{Title}'",
                updated.Id, updated.Title);

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
        int id, IOpenApiService openApiService, ILogger<Program> logger)
    {
        try
        {
            var deleted = await openApiService.DeleteAsync(id);
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
        int id, IOpenApiService openApiService, IMockRouteService mockRouteService, ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Generate mocks called for document ID: {DocumentId}", id);
            
            var document = await openApiService.GetByIdAsync(id);
            if (document == null)
            {
                logger.LogWarning("Document {DocumentId} not found", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Found document {DocumentId}, starting OpenAPI parsing", id);

            // Parse the OpenAPI document
            var reader = new OpenApiStringReader();
            var openApiDocument = reader.Read(document.Content, out var diagnostic);
            
            logger.LogInformation("OpenAPI document parsed. Errors: {ErrorCount}, Warnings: {WarningCount}", 
                diagnostic.Errors.Count, diagnostic.Warnings.Count);
            
            if (diagnostic.Errors.Any())
            {
                logger.LogWarning("OpenAPI document has errors: {Errors}", 
                    string.Join(", ", diagnostic.Errors.Select(e => e.Message)));
                // Continue anyway, might still be usable
            }

            var mockRoutes = new List<MockRouteResponse>();

            logger.LogInformation("Found {PathCount} paths in OpenAPI document", openApiDocument.Paths?.Count ?? 0);

            // Generate mock routes for each path and operation
            if (openApiDocument.Paths != null)
            {
                foreach (var path in openApiDocument.Paths)
                {
                    logger.LogInformation("Processing path: {Path} with {OperationCount} operations", 
                        path.Key, path.Value.Operations?.Count ?? 0);
                    
                    foreach (var operation in path.Value.Operations)
                    {
                        try
                        {
                            var method = operation.Key.ToString().ToUpper();
                            var pathPattern = path.Key;
                            
                            logger.LogInformation("Generating mock for {Method} {Path}", method, pathPattern);
                            
                            // Generate simple mock response for now
                            var mockResponse = new 
                            { 
                                message = $"Mock response for {method} {pathPattern}",
                                timestamp = DateTime.UtcNow,
                                operationId = operation.Value.OperationId ?? "unknown"
                            };
                            
                            // Create mock route entity
                            var mockRouteEntity = new Backend.Core.Domain.Entities.MariaDb.MockRoute
                            {
                                RouteId = Guid.NewGuid().ToString(),
                                Method = method,
                                Path = pathPattern,
                                HttpStatusCode = 200,
                                Mock = JsonSerializer.Serialize(mockResponse),
                                Enabled = true
                            };

                            logger.LogInformation("Saving mock route to database: {Method} {Path}", method, pathPattern);
                            
                            try
                            {
                                // Save to database
                                var createdRoute = await mockRouteService.CreateAsync(mockRouteEntity);
                                
                                // Convert to response DTO
                                var mockRouteResponse = new MockRouteResponse
                                {
                                    Id = createdRoute.Id,
                                    RouteId = createdRoute.RouteId ?? string.Empty,
                                    Method = createdRoute.Method ?? string.Empty,
                                    Path = createdRoute.Path ?? string.Empty,
                                    HttpStatusCode = createdRoute.HttpStatusCode,
                                    Mock = createdRoute.Mock,
                                    Enabled = createdRoute.Enabled,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                
                                mockRoutes.Add(mockRouteResponse);
                                logger.LogInformation("Successfully generated mock route: {Method} {Path}", method, pathPattern);
                            }
                            catch (InvalidOperationException duplicateEx)
                            {
                                logger.LogWarning("Skipping duplicate mock route: {Method} {Path} - {Message}", 
                                    method, pathPattern, duplicateEx.Message);
                            }
                        }
                        catch (Exception operationEx)
                        {
                            logger.LogError(operationEx, "Failed to generate mock for operation {Method} {Path}", 
                                operation.Key.ToString().ToUpper(), path.Key);
                            // Continue with next operation
                        }
                    }
                }
            }

            logger.LogInformation("Successfully generated {Count} mock routes from OpenAPI document {DocumentId}",
                mockRoutes.Count, id);

            return TypedResults.Created($"/prock/api/mock-routes", mockRoutes.AsEnumerable());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate mocks from OpenAPI document {DocumentId}", id);
            return TypedResults.Problem($"Failed to generate mocks from OpenAPI document: {ex.Message}", statusCode: 500);
        }
    }

    private static object GenerateMockResponse(OpenApiOperation operation, OpenApiDocument document)
    {
        // Try to find a 200 response, fallback to first response
        OpenApiResponse? response = null;
        
        if (operation.Responses.TryGetValue("200", out response) ||
            operation.Responses.TryGetValue("201", out response))
        {
            // Found a specific success response
        }
        else if (operation.Responses.Values.FirstOrDefault() is var firstResponse && firstResponse != null)
        {
            response = firstResponse;
        }
        else
        {
            // Return a generic success response if no response schema found
            return new { message = "Success", timestamp = DateTime.UtcNow };
        }

        // Try to get JSON content from the response
        if (response?.Content?.TryGetValue("application/json", out var mediaType) == true && 
            mediaType.Schema != null)
        {
            return GenerateMockFromSchema(mediaType.Schema, document);
        }

        // Fallback to generic response
        return new { message = "Success", timestamp = DateTime.UtcNow };
    }

    private static object GenerateMockFromSchema(OpenApiSchema schema, OpenApiDocument document)
    {
        // Handle schema references
        if (!string.IsNullOrEmpty(schema.Reference?.Id))
        {
            if (document.Components?.Schemas?.TryGetValue(schema.Reference.Id, out var referencedSchema) == true)
            {
                return GenerateMockFromSchema(referencedSchema, document);
            }
        }

        // Handle different schema types
        return schema.Type?.ToLower() switch
        {
            "object" => GenerateMockObject(schema, document),
            "array" => GenerateMockArray(schema, document),
            "string" => GenerateMockString(schema),
            "integer" => GenerateMockInteger(schema),
            "number" => GenerateMockNumber(schema),
            "boolean" => true,
            _ => schema.Example?.ToString() ?? "mock_value"
        };
    }

    private static object GenerateMockObject(OpenApiSchema schema, OpenApiDocument document)
    {
        var obj = new Dictionary<string, object>();

        if (schema.Properties?.Any() == true)
        {
            foreach (var property in schema.Properties)
            {
                obj[property.Key] = GenerateMockFromSchema(property.Value, document);
            }
        }
        else
        {
            // Generic object if no properties defined
            obj["id"] = 1;
            obj["name"] = "Example Name";
            obj["timestamp"] = DateTime.UtcNow;
        }

        return obj;
    }

    private static object GenerateMockArray(OpenApiSchema schema, OpenApiDocument document)
    {
        var list = new List<object>();
        var itemSchema = schema.Items;
        
        if (itemSchema != null)
        {
            // Generate 2-3 mock items
            for (int i = 0; i < 2; i++)
            {
                list.Add(GenerateMockFromSchema(itemSchema, document));
            }
        }
        else
        {
            // Fallback array
            list.Add(new { id = 1, name = "Item 1" });
            list.Add(new { id = 2, name = "Item 2" });
        }

        return list;
    }

    private static object GenerateMockString(OpenApiSchema schema)
    {
        if (schema.Enum?.Any() == true)
        {
            return schema.Enum.First()?.ToString() ?? "enum_value";
        }

        if (!string.IsNullOrEmpty(schema.Format))
        {
            return schema.Format.ToLower() switch
            {
                "date" => DateTime.Today.ToString("yyyy-MM-dd"),
                "date-time" => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                "email" => "example@example.com",
                "uri" => "https://example.com",
                "uuid" => Guid.NewGuid().ToString(),
                _ => schema.Example?.ToString() ?? "example_string"
            };
        }

        return schema.Example?.ToString() ?? "example_string";
    }

    private static object GenerateMockInteger(OpenApiSchema schema)
    {
        if (schema.Minimum.HasValue)
        {
            return (int)schema.Minimum.Value;
        }
        
        return schema.Example?.ToString() ?? "123";
    }

    private static object GenerateMockNumber(OpenApiSchema schema)
    {
        if (schema.Minimum.HasValue)
        {
            return schema.Minimum.Value;
        }
        
        return schema.Example?.ToString() ?? "123.45";
    }

    // Mapping helpers
    private static OpenApiDocumentResponse MapToResponse(Backend.Core.Domain.Entities.MariaDb.OpenApiSpecification entity)
    {
        return new OpenApiDocumentResponse
        {
            DocumentId = entity.Id.ToString(),
            Title = entity.Title ?? string.Empty,
            Version = entity.Version,
            Description = entity.Description,
            OpenApiVersion = entity.OpenApiVersion,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
            IsActive = entity.IsActive,
            PathCount = 0 // TODO: Calculate from OpenApiPath entities if needed
        };
    }

    private static Backend.Core.Domain.Entities.MariaDb.OpenApiSpecification MapToEntity(CreateOpenApiDocumentRequest request)
    {
        return new Backend.Core.Domain.Entities.MariaDb.OpenApiSpecification
        {
            Title = request.Title,
            Version = request.Version,
            Description = request.Description,
            Content = request.OriginalJson,
            IsActive = true,
        };
    }
}
