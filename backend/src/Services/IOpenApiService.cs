using backend.Data.Dto;
using backend.Data.Entities;
using backend.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.Services;

public interface IOpenApiService
{
    Task<Ok<List<OpenApiDocumentDto>>> GetAllDocumentsAsync();
    Task<Results<Ok<OpenApiDocumentDetailDto>, NotFound>> GetDocumentByIdAsync(Guid documentId);
    Task<Results<Ok<OpenApiDocumentDto>, NotFound, BadRequest<string>>> UpdateDocumentAsync(Guid documentId, UpdateOpenApiDocumentDto request);
    Task<Results<Created<OpenApiDocumentDto>, BadRequest<string>>> CreateDocumentAsync(CreateOpenApiDocumentDto request);
    Task<Results<Ok<List<MockRouteDto>>, NotFound, BadRequest<string>>> GenerateMockRoutesAsync(Guid documentId);
    Task<Results<NoContent, NotFound>> DeleteDocumentAsync(Guid documentId);
}
