using backend.Data;
using backend.Data.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Readers;
using OpenApiSpecification = backend.Data.Entities.OpenApiSpecification;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.OpenApi.Models;
using backend.Data.Entities;

namespace backend.Endpoints;

public static class OpenApiEndpoints
{

    public static void RegisterOpenApiEndpoints(this WebApplication app)
    {
        // Get all OpenAPI documents
        app.MapGet("/prock/api/openapi-documents",
            async Task<Results<Ok<List<OpenApiDocumentDto>>, Ok>> (ProckDbContext db) =>
            await db.OpenApiDocuments.Where(x => x.IsActive).ToListAsync() is List<OpenApiSpecification> documents
                ? TypedResults.Ok(documents.Select(doc => new OpenApiDocumentDto
                {
                    DocumentId = doc.DocumentId,
                    Title = doc.Title,
                    Version = doc.Version,
                    Description = doc.Description,
                    OpenApiVersion = doc.OpenApiVersion,
                    BasePath = doc.BasePath,
                    Host = doc.Host,
                    Schemes = doc.Schemes,
                    Consumes = doc.Consumes,
                    Produces = doc.Produces,
                    CreatedAt = doc.CreatedAt,
                    UpdatedAt = doc.UpdatedAt,
                    IsActive = doc.IsActive
                }).ToList())
                : TypedResults.Ok());

        // Get specific OpenAPI document by ID
        app.MapGet("/prock/api/openapi-documents/{documentId}",
            async Task<Results<Ok<OpenApiDocumentDetailDto>, NotFound>> (Guid documentId, ProckDbContext db) =>
                await db.GetOpenApiDocumentByIdAsync(documentId) is OpenApiSpecification document
                    ? TypedResults.Ok(new OpenApiDocumentDetailDto
                    {
                        DocumentId = document.DocumentId,
                        Title = document.Title,
                        Version = document.Version,
                        Description = document.Description,
                        OpenApiVersion = document.OpenApiVersion,
                        BasePath = document.BasePath,
                        Host = document.Host,
                        Schemes = document.Schemes,
                        Consumes = document.Consumes,
                        Produces = document.Produces,
                        CreatedAt = document.CreatedAt,
                        UpdatedAt = document.UpdatedAt,
                        IsActive = document.IsActive,
                        Paths = null, // Simplified for now - can parse from PathsData if needed
                        Tags = null, // Simplified for now - can parse from TagsData if needed  
                        Servers = null, // Simplified for now - can parse from ServersData if needed
                        OriginalJson = document.OriginalJson
                    })
                    : TypedResults.NotFound());

        // Create new OpenAPI document
        app.MapPost("/prock/api/openapi-documents",
            async Task<Results<Created<OpenApiDocumentDto>, BadRequest<string>>> (CreateOpenApiDocumentDto request, ProckDbContext db) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(request.OpenApiJson))
                    {
                        return TypedResults.BadRequest("OpenAPI JSON is required");
                    }

                    // Parse the OpenAPI JSON to validate and extract information
                    var msOpenApiDocument = ParseOpenApiJson(request.OpenApiJson);
                    if (msOpenApiDocument == null)
                    {
                        return TypedResults.BadRequest("Invalid OpenAPI JSON format");
                    }
                    var oasDocumentId = Guid.NewGuid();

                    var entity = new OpenApiSpecification
                    {
                        DocumentId = oasDocumentId,
                        Title = request.Title ?? msOpenApiDocument.Info?.Title ?? "Untitled API",
                        Version = request.Version ?? msOpenApiDocument.Info?.Version ?? "1.0.0",
                        Description = request.Description ?? msOpenApiDocument.Info?.Description,
                        OpenApiVersion = "3.0.0",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true,
                        OriginalJson = request.OpenApiJson,
                        Paths = [.. msOpenApiDocument.Paths.Select(path => new OpenApiPath
                        {
                            Path = path.Key,
                            Description = path.Value.Description,
                            Summary = path.Value.Summary,
                        })],
                    };

                    // Extract additional information from the parsed document
                    //ExtractOpenApiInformation(entity, msOpenApiDocument);

                    db.OpenApiDocuments.Add(entity);
                    await db.SaveChangesAsync();

                    var result = new OpenApiDocumentDto
                    {
                        DocumentId = entity.DocumentId,
                        Title = entity.Title,
                        Version = entity.Version,
                        Description = entity.Description,
                        OpenApiVersion = entity.OpenApiVersion,
                        BasePath = entity.BasePath,
                        Host = entity.Host,
                        CreatedAt = entity.CreatedAt,
                        UpdatedAt = entity.UpdatedAt,
                        IsActive = entity.IsActive,
                        Paths = [.. entity.Paths.Select(p => new OpenApiPathDto
                        {
                            Path = p.Path,
                            Summary = p.Summary,
                            Description = p.Description
                        })]


                    };

                    return TypedResults.Created($"/prock/api/openapi-documents/{entity.DocumentId}", result);
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest($"Error processing OpenAPI document: {ex.Message}");
                }
            });

        // Update OpenAPI document
        app.MapPut("/prock/api/openapi-documents/{documentId}",
            async Task<Results<Ok<OpenApiDocumentDto>, NotFound, BadRequest<string>>> (Guid documentId, UpdateOpenApiDocumentDto request, ProckDbContext db) =>
            {
                var entity = await db.GetOpenApiDocumentByIdAsync(documentId);
                if (entity == null)
                {
                    return TypedResults.NotFound();
                }

                try
                {
                    if (!string.IsNullOrEmpty(request.OpenApiJson))
                    {
                        var msOpenApiDocument = ParseOpenApiJson(request.OpenApiJson);
                        if (msOpenApiDocument == null)
                        {
                            return TypedResults.BadRequest("Invalid OpenAPI JSON format");
                        }
                        entity.OriginalJson = request.OpenApiJson;
                        //ExtractOpenApiInformation(entity, msOpenApiDocument);
                    }

                    entity.Title = request.Title ?? entity.Title;
                    entity.Version = request.Version ?? entity.Version;
                    entity.Description = request.Description ?? entity.Description;
                    entity.IsActive = request.IsActive ?? entity.IsActive;
                    entity.UpdatedAt = DateTime.UtcNow;

                    await db.SaveChangesAsync();

                    var result = new OpenApiDocumentDto
                    {
                        DocumentId = entity.DocumentId,
                        Title = entity.Title,
                        Version = entity.Version,
                        Description = entity.Description,
                        OpenApiVersion = entity.OpenApiVersion,
                        BasePath = entity.BasePath,
                        Host = entity.Host,
                        // Schemes = entity.Schemes,
                        // Consumes = entity.Consumes,
                        // Produces = entity.Produces,
                        CreatedAt = entity.CreatedAt,
                        UpdatedAt = entity.UpdatedAt,
                        IsActive = entity.IsActive
                    };

                    return TypedResults.Ok(result);
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest($"Error updating OpenAPI document: {ex.Message}");
                }
            });

        // Delete OpenAPI document (soft delete)
        app.MapDelete("/prock/api/openapi-documents/{documentId}",
            async Task<Results<NoContent, NotFound>> (Guid documentId, ProckDbContext db) =>
            {
                var entity = await db.GetOpenApiDocumentByIdAsync(documentId);
                if (entity == null)
                {
                    return TypedResults.NotFound();
                }

                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                return TypedResults.NoContent();
            });
        // Get OpenAPI JSON for a specific document
        app.MapGet("/prock/api/openapi-documents/{documentId}/json",
            async Task<Results<Ok<OpenApiDocument>, BadRequest<string>, NotFound>> (Guid documentId, ProckDbContext db) =>
            {
                return await db.GetOpenApiDocumentByIdAsync(documentId) is OpenApiSpecification document && !string.IsNullOrEmpty(document.OriginalJson)
                    ? ParseOpenApiJson(document.OriginalJson) is OpenApiDocument parsedDoc
                        ? TypedResults.Ok(parsedDoc)
                        : TypedResults.BadRequest("Invalid OpenAPI JSON format")
                    : TypedResults.NotFound();
            });
     
app.MapPost("/prock/api/openapi-documents/{documentId}/generate-mock-routes",
    async Task<Results<Ok<List<MockRouteDto>>, NotFound, BadRequest<string>>> (Guid documentId, ProckDbContext db) =>
    {
        var document = await db.GetOpenApiDocumentByIdAsync(documentId);
        if (document == null)
            return TypedResults.NotFound();

        if (string.IsNullOrEmpty(document.OriginalJson))
            return TypedResults.BadRequest("No OpenAPI JSON found for this document.");

        // Parse OpenAPI JSON
        var openApiDoc = ParseOpenApiJson(document.OriginalJson);
        if (openApiDoc == null)
            return TypedResults.BadRequest("Invalid OpenAPI JSON.");

        var createdRoutes = new List<MockRouteDto>();

        // For each path and method, create a mock route
        foreach (var path in openApiDoc.Paths)
        {
            foreach (var op in path.Value.Operations)
            {
                var mockRoute = new MockRoute
                {
                    RouteId = Guid.NewGuid(),
                    Path = path.Key,
                    Method = op.Key.ToString().ToUpper(),
                    HttpStatusCode = 200,
                    Mock = @"{}", // You can customize this as needed
                    Enabled = true
                };
                db.MockRoutes.Add(mockRoute);
                await db.SaveChangesAsync();
                createdRoutes.Add(new MockRouteDto
                {
                    RouteId = mockRoute.RouteId,
                    Path = mockRoute.Path,
                    Method = mockRoute.Method,
                    HttpStatusCode = mockRoute.HttpStatusCode,
                    Mock = mockRoute.Mock,
                    Enabled = mockRoute.Enabled
                });
            }
        }
        
        return TypedResults.Ok(createdRoutes);
    });

    }

    private static OpenApiDocument? ParseOpenApiJson(string json)
    {
        Console.WriteLine($"OpenAPI document parse attempt for JSON of length {json.Length}");
        try
        {
            var reader = new OpenApiStringReader(settings: new OpenApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences,
                // LoadExternalRefs = false,
            });
            
            var openApiDocument = reader.Read(json, out var diagnostic);

            if (diagnostic.Errors.Count > 0)
            {
                Console.WriteLine($"OpenAPI document parsing errors: {string.Join(", ", diagnostic.Errors.Select(e => e.Message))}");
                return null;
            }
            Console.WriteLine($"OpenAPI document parse attempt success!");
            return openApiDocument;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error parsing OpenAPI JSON: {ex.Message}");
            return null;
        }
    }

    private static void ExtractOpenApiInformation(OpenApiSpecification entity, OpenApiDocument parsedDoc)
    {
        try
        {
            // Extract servers and store as BsonDocument
            if (parsedDoc.Servers?.Count > 0)
            {
                var serversJson = JsonSerializer.Serialize(parsedDoc.Servers.Select(s => new
                {
                    Url = s.Url,
                    Description = s.Description,
                    Variables = s.Variables?.ToDictionary(kv => kv.Key, kv => new
                    {
                        Default = kv.Value.Default,
                        Description = kv.Value.Description,
                        Enum = kv.Value.Enum
                    })
                }));
                //entity.ServersData = BsonDocument.Parse(serversJson);

                // Set legacy fields for backward compatibility
                var firstServer = parsedDoc.Servers.First();
                if (Uri.TryCreate(firstServer.Url, UriKind.Absolute, out var uri))
                {
                    entity.Host = uri.Host;
                    entity.Schemes = new List<string> { uri.Scheme };
                    entity.BasePath = uri.AbsolutePath;
                }
            }

            // Extract tags and store as BsonDocument
            if (parsedDoc.Tags?.Count > 0)
            {
                var tagsJson = JsonSerializer.Serialize(parsedDoc.Tags.Select(t => new
                {
                    Name = t.Name,
                    Description = t.Description,
                    ExternalDocs = t.ExternalDocs != null ? new
                    {
                        Description = t.ExternalDocs.Description,
                        Url = t.ExternalDocs.Url?.ToString()
                    } : null
                }));
                //entity.TagsData = BsonDocument.Parse(tagsJson);
            }

            // Extract paths and operations and store as BsonDocument
            if (parsedDoc.Paths?.Count > 0)
            {
                var pathsJson = JsonSerializer.Serialize(parsedDoc.Paths.ToDictionary(
                    pathPair => pathPair.Key,
                    pathPair => new
                    {
                        Operations = pathPair.Value.Operations?.ToDictionary(
                            opPair => opPair.Key.ToString().ToUpper(),
                            opPair => new
                            {
                                OperationId = opPair.Value.OperationId,
                                Summary = opPair.Value.Summary,
                                Description = opPair.Value.Description,
                                Tags = opPair.Value.Tags?.Select(t => t.Name).ToList(),
                                Deprecated = opPair.Value.Deprecated
                            })
                    }));
                //entity.PathsData = BsonDocument.Parse(pathsJson);
            }

            // Extract components and store as BsonDocument
            if (parsedDoc.Components != null)
            {
                var componentsJson = JsonSerializer.Serialize(new
                {
                    Schemas = parsedDoc.Components.Schemas?.ToDictionary(
                        kv => kv.Key,
                        kv => new { 
                            Type = kv.Value.Type,
                            Description = kv.Value.Description,
                            Properties = kv.Value.Properties?.Keys.ToList()
                        })
                });
                //entity.ComponentsData = BsonDocument.Parse(componentsJson);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail - we still have the original JSON
            Console.WriteLine($"Error extracting OpenAPI information: {ex.Message}");
        }
    }
}
