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
            async Task<Results<Ok<MsOpenApi.OpenApiDocument>, BadRequest<string>, NotFound>> (Guid documentId, ProckDbContext db) =>
            {
                return await db.GetOpenApiDocumentByIdAsync(documentId) is OpenApiSpecification document && !string.IsNullOrEmpty(document.OriginalJson)
                    ? ParseOpenApiJson(document.OriginalJson) is MsOpenApi.OpenApiDocument parsedDoc
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
        var newMockRoutes = new List<MockRoute>();
        int routeCount = 0;
        var random = new Random();

        // For each path and method, create a mock route
        foreach (var path in openApiDoc.Paths)
        {
            foreach (var op in path.Value.Operations)
            {
                Console.WriteLine($"[MockGen] Processing {op.Key} {path.Key}");
                // Try to find a success response (200, 201, or default)
                MsOpenApi.OpenApiResponse? response = null;
                int statusCode = 200;

                if (op.Value.Responses.TryGetValue("200", out var response200))
                {
                    response = response200;
                    statusCode = 200;
                }
                else if (op.Value.Responses.TryGetValue("201", out var response201))
                {
                    response = response201;
                    statusCode = 201;
                }
                else if (op.Value.Responses.TryGetValue("default", out var responseDefault))
                {
                    response = responseDefault;
                    statusCode = 200; // Default to 200 for 'default' response if strictly specific status not found
                }
                else
                {
                     Console.WriteLine($"[MockGen] No 200/201/default response found. Available: {string.Join(", ", op.Value.Responses.Keys)}");
                }

                string mockBody = "{}";

                if (response != null)
                {
                    if (response.Content.TryGetValue("application/json", out var mediaType))
                    {
                        if (mediaType.Schema != null)
                        {
                            Console.WriteLine($"[MockGen] Found schema for {op.Key} {path.Key}. Calling GenerateMockValue.");
                            try 
                            {
                                var mockData = GenerateMockValue(mediaType.Schema, openApiDoc, random);
                                
                                if (mockData == null) 
                                {
                                    Console.WriteLine($"[MockGen] GenerateMockValue returned null for {op.Key} {path.Key}");
                                }
                                else 
                                {
                                    var typeName = mockData.GetType().FullName;
                                    Console.WriteLine($"[MockGen] GenerateMockValue returned type: {typeName}");
                                    
                                    if (typeName.Contains("Microsoft.OpenApi"))
                                    {
                                        Console.WriteLine($"[MockGen] CRITICAL: Returned internal OpenAPI type! Aborting serialization.");
                                        mockBody = "{}";
                                    }
                                    else
                                    {
                                        mockBody = JsonSerializer.Serialize(mockData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[MockGen] Serialization Error for {op.Key} {path.Key}: {ex.Message}");
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[MockGen] Schema is null for {op.Key} {path.Key}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[MockGen] 'application/json' not found. Available: {string.Join(", ", response.Content.Keys)}");
                    }
                }

                var mockRoute = new MockRoute
                {
                    RouteId = Guid.NewGuid(),
                    Path = path.Key,
                    Method = op.Key.ToString().ToUpper(),
                    HttpStatusCode = statusCode,
                    Mock = mockBody,
                    Enabled = true
                };
                newMockRoutes.Add(mockRoute);
                createdRoutes.Add(new MockRouteDto
                {
                    RouteId = mockRoute.RouteId,
                    Path = mockRoute.Path,
                    Method = mockRoute.Method,
                    HttpStatusCode = mockRoute.HttpStatusCode,
                    Mock = mockRoute.Mock,
                    Enabled = mockRoute.Enabled
                });
                routeCount++;
            }
        }
        db.MockRoutes.AddRange(newMockRoutes);
        try
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[INFO] Generated {routeCount} mock routes from OpenAPI document {documentId} at {DateTime.UtcNow:O}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to save generated mock routes: {ex.Message}");
            return TypedResults.BadRequest($"Failed to save generated mock routes: {ex.Message}");
        }
        return TypedResults.Ok(createdRoutes);
    });

    }

    private static object? GenerateMockValue(MsOpenApi.OpenApiSchema schema, MsOpenApi.OpenApiDocument doc, Random random, int depth = 0, string? parentRefId = null)
    {
        if (depth > 5) return null; 

        if (schema.Reference != null)
        {
            var refId = schema.Reference.Id;
            
            // Handle self-reference / recursion prevention
            if (refId == parentRefId)
            {
                // Fall through to process the schema structure itself
            }
            else
            {
                if (doc.Components != null && doc.Components.Schemas.TryGetValue(refId, out var refSchema))
                {
                    return GenerateMockValue(refSchema, doc, random, depth + 1, refId);
                }
                
                var simpleName = refId.Split('/').Last();
                if (doc.Components != null && doc.Components.Schemas.TryGetValue(simpleName, out var refSchemaSimple))
                {
                     return GenerateMockValue(refSchemaSimple, doc, random, depth + 1, simpleName);
                }

                return null; 
            }
        }

        switch (schema.Type)
        {
            case "object":
                var obj = new Dictionary<string, object?>();
                if (schema.Properties != null)
                {
                    foreach (var prop in schema.Properties)
                    {
                        obj[prop.Key] = GenerateMockValue(prop.Value, doc, random, depth + 1);
                    }
                }
                return obj;

            case "array":
                var items = new List<object?>();
                int count = random.Next(1, 4); 
                for (int i = 0; i < count; i++)
                {
                    items.Add(GenerateMockValue(schema.Items, doc, random, depth + 1));
                }
                return items;

            case "string":
                if (schema.Enum != null && schema.Enum.Count > 0)
                {
                    var index = random.Next(schema.Enum.Count);
                    var enumVal = schema.Enum[index];
                    if (enumVal is Microsoft.OpenApi.Any.OpenApiString s) return s.Value;
                    if (enumVal is Microsoft.OpenApi.Any.OpenApiInteger i) return i.Value;
                    return enumVal.ToString();
                }

                if (schema.Format == "date-time") return DateTime.UtcNow.ToString("O");
                if (schema.Format == "date") return DateTime.UtcNow.ToString("yyyy-MM-dd");
                if (schema.Format == "uuid") return Guid.NewGuid().ToString();

                var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetur" };
                return $"{words[random.Next(words.Length)]}_{random.Next(1000)}";

            case "integer":
                return random.Next(1, 100);

            case "number":
                return random.NextDouble() * 100.0;

            case "boolean":
                return random.Next(2) == 0;

            default:
                if (schema.Properties != null && schema.Properties.Count > 0)
                {
                    var implicitObj = new Dictionary<string, object?>();
                    foreach (var prop in schema.Properties)
                    {
                         implicitObj[prop.Key] = GenerateMockValue(prop.Value, doc, random, depth + 1);
                    }
                    return implicitObj;
                }
                return null;
        }
    }

    private static MsOpenApi.OpenApiDocument? ParseOpenApiJson(string json)
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
            
            // Explicitly verify the type since generic reader might return something else if configured differently
            // though here using string reader for OpenApiDocument
            
            if (diagnostic.Errors.Count > 0)
            {
                Console.WriteLine($"OpenAPI document parsing errors: {string.Join(", ", diagnostic.Errors.Select(e => e.Message))}");
                return null;
            }
            Console.WriteLine($"OpenAPI document parse attempt success!");
            return openApiDocument;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing OpenAPI JSON: {ex.Message}");
            return null;
        }
    }

    private static void ExtractOpenApiInformation(OpenApiSpecification entity, MsOpenApi.OpenApiDocument parsedDoc)
    {
        try
        {
            // Extract servers and store as BsonDocument
            if (parsedDoc.Servers?.Count > 0)
            {
                var serversJson = JsonSerializer.Serialize(parsedDoc.Servers.Select(s => new
                {
                    s.Url,
                    s.Description,
                    Variables = s.Variables?.ToDictionary(kv => kv.Key, kv => new
                    {
                        kv.Value.Default,
                        kv.Value.Description,
                        kv.Value.Enum
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
                    t.Name,
                    t.Description,
                    ExternalDocs = t.ExternalDocs != null ? new
                    {
                        t.ExternalDocs.Description,
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
                                opPair.Value.OperationId,
                                opPair.Value.Summary,
                                opPair.Value.Description,
                                Tags = opPair.Value.Tags?.Select(t => t.Name).ToList(),
                                opPair.Value.Deprecated
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
                        kv => new
                        {
                            kv.Value.Type,
                            kv.Value.Description,
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
