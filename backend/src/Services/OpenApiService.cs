using System.Text.Json;
using backend.Data.Dto;
using backend.Data.Entities;
using backend.Endpoints;
using backend.Repositories;
using backend.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using MsOpenApi = Microsoft.OpenApi.Models;

namespace backend.Services;

public class OpenApiService : IOpenApiService
{
    private readonly IOpenApiRepository _repository;
    private readonly IMockRouteRepository _mockRouteRepository;

    public OpenApiService(IOpenApiRepository repository, IMockRouteRepository mockRouteRepository)
    {
        _repository = repository;
        _mockRouteRepository = mockRouteRepository;
    }

    public async Task<Ok<List<OpenApiDocumentDto>>> GetAllDocumentsAsync()
    {
        var documents = await _repository.GetAllDocumentsAsync();
        return TypedResults.Ok(documents);
    }

    public async Task<Results<Ok<OpenApiDocumentDetailDto>, NotFound>> GetDocumentByIdAsync(Guid documentId)
    {
        var document = await _repository.GetDocumentByIdAsync(documentId);
        
        if (document == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(document);
    }

    public async Task<Results<Created<OpenApiDocumentDto>, BadRequest<string>>> CreateDocumentAsync(CreateOpenApiDocumentDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.OpenApiJson))
            {
                return TypedResults.BadRequest("OpenAPI JSON is required");
            }

            var result = await _repository.CreateDocumentAsync(request);
            return TypedResults.Created($"/prock/api/openapi-documents/{result.DocumentId}", result);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest($"Error processing OpenAPI document: {ex.Message}");
        }
    }

    public async Task<Results<Ok<OpenApiDocumentDto>, NotFound, BadRequest<string>>> UpdateDocumentAsync(Guid documentId, UpdateOpenApiDocumentDto request)
    {
        try
        {
            var result = await _repository.UpdateDocumentAsync(documentId, request);
            if (result == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest($"Error updating OpenAPI document: {ex.Message}");
        }
    }

    public async Task<Results<Ok<List<MockRouteDto>>, NotFound, BadRequest<string>>> GenerateMockRoutesAsync(Guid documentId)
    {
        var document = await _repository.GetDocumentByIdAsync(documentId);
        if (document == null)
            return TypedResults.NotFound();

        if (string.IsNullOrEmpty(document.OriginalJson))
            return TypedResults.BadRequest("No OpenAPI JSON found for this document.");

        // Parse OpenAPI JSON
        var openApiDoc = MockDataGenerator.ParseOpenApiJson(document.OriginalJson);
        if (openApiDoc == null)
            return TypedResults.BadRequest("Invalid OpenAPI JSON.");

        var createdRoutes = new List<MockRouteDto>();
        int routeCount = 0;
        var random = new Random();

        // For each path and method, create a mock route
        foreach (var path in openApiDoc.Paths)
        {
            foreach (var op in path.Value.Operations)
            {
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
                    // No standard success response
                }

                string mockBody = "{}";

                if (response != null)
                {
                    if (response.Content.TryGetValue("application/json", out var mediaType))
                    {
                        if (mediaType.Schema != null)
                        {
                            try 
                            {
                                var mockData = MockDataGenerator.GenerateMockValue(mediaType.Schema, openApiDoc, random);
                                
                                if (mockData != null) 
                                {
                                    var typeName = mockData.GetType().FullName;
                                    
                                    if (typeName != null && typeName.Contains("Microsoft.OpenApi"))
                                    {
                                        mockBody = "{}";
                                    }
                                    else
                                    {
                                        mockBody = JsonSerializer.Serialize(mockData, new JsonSerializerOptions { WriteIndented = true });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[MockGen] Serialization Error for {op.Key} {path.Key}: {ex.Message}");
                            }
                        }
                    }
                }

                var mockRouteDto = new MockRouteDto
                {
                    RouteId = Guid.NewGuid(),
                    Path = path.Key,
                    Method = op.Key.ToString().ToUpper(),
                    HttpStatusCode = statusCode,
                    Mock = !string.IsNullOrEmpty(mockBody) ? JsonSerializer.Deserialize<dynamic>(mockBody) : null,
                    Enabled = true
                };
                
                createdRoutes.Add(mockRouteDto);
                routeCount++;
            }
        }
        
        try
        {
            await _mockRouteRepository.CreateRoutesAsync(createdRoutes);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest($"Failed to save generated mock routes: {ex.Message}");
        }
        return TypedResults.Ok(createdRoutes);
    }

    public async Task<Results<NoContent, NotFound>> DeleteDocumentAsync(Guid documentId)
    {
        var exists = await _repository.GetDocumentByIdAsync(documentId);
        if (exists == null) 
        {
            return TypedResults.NotFound();
        }

        await _repository.DeleteDocumentAsync(documentId);
        return TypedResults.NoContent();
    }
}
