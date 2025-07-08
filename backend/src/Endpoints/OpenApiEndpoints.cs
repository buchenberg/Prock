using System.Text.Json;
using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Readers;
using MongoDB.Bson;
using OpenApiDocument = backend.Data.Entities.OpenApiDocument;
using MsOpenApiDocument = Microsoft.OpenApi.Models.OpenApiDocument;

namespace backend.Endpoints;

public static class OpenApiEndpoints
{
    public static void RegisterOpenApiEndpoints(this WebApplication app)
    {
        // Get all OpenAPI documents
        app.MapGet("/prock/api/openapi-documents", 
            async Task<Results<Ok<List<OpenApiDocumentDto>>, Ok>> (ProckDbContext db) =>
            await db.OpenApiDocuments.Where(x => x.IsActive).ToListAsync() is List<OpenApiDocument> documents
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
                await db.GetOpenApiDocumentByIdAsync(documentId) is OpenApiDocument document
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

                    var entity = new OpenApiDocument
                    {
                        DocumentId = Guid.NewGuid(),
                        Title = request.Title ?? msOpenApiDocument.Info?.Title ?? "Untitled API",
                        Version = request.Version ?? msOpenApiDocument.Info?.Version ?? "1.0.0",
                        Description = request.Description ?? msOpenApiDocument.Info?.Description,
                        OpenApiVersion = "3.0.0", // Default to 3.0.0, can be extracted from document if needed
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true,
                        OriginalJson = request.OpenApiJson
                    };

                    // Extract additional information from the parsed document
                    ExtractOpenApiInformation(entity, msOpenApiDocument);

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
                        Schemes = entity.Schemes,
                        Consumes = entity.Consumes,
                        Produces = entity.Produces,
                        CreatedAt = entity.CreatedAt,
                        UpdatedAt = entity.UpdatedAt,
                        IsActive = entity.IsActive
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
                        ExtractOpenApiInformation(entity, msOpenApiDocument);
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
                        Schemes = entity.Schemes,
                        Consumes = entity.Consumes,
                        Produces = entity.Produces,
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
            async Task<Results<Ok<string>, NotFound>> (Guid documentId, ProckDbContext db) =>
                await db.GetOpenApiDocumentByIdAsync(documentId) is OpenApiDocument document && !string.IsNullOrEmpty(document.OriginalJson)
                    ? TypedResults.Ok(document.OriginalJson)
                    : TypedResults.NotFound());
    }

    private static MsOpenApiDocument? ParseOpenApiJson(string json)
    {
        try
        {
            var reader = new OpenApiStringReader();
            var openApiDocument = reader.Read(json, out var diagnostic);
            
            if (diagnostic.Errors.Count > 0)
            {
                return null;
            }
            
            return openApiDocument;
        }
        catch
        {
            return null;
        }
    }

    private static void ExtractOpenApiInformation(OpenApiDocument entity, MsOpenApiDocument parsedDoc)
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
                entity.ServersData = BsonDocument.Parse(serversJson);

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
                entity.TagsData = BsonDocument.Parse(tagsJson);
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
                entity.PathsData = BsonDocument.Parse(pathsJson);
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
                entity.ComponentsData = BsonDocument.Parse(componentsJson);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail - we still have the original JSON
            Console.WriteLine($"Error extracting OpenAPI information: {ex.Message}");
        }
    }
}
