using backend.Data.Dto;
using backend.Data.Entities;
using MsOpenApi = Microsoft.OpenApi.Models;

namespace backend.Repositories;

public interface IOpenApiRepository
{
    Task<List<OpenApiDocumentDto>> GetAllDocumentsAsync();
    Task<OpenApiDocumentDetailDto?> GetDocumentByIdAsync(Guid documentId);
    Task<OpenApiDocumentDto> CreateDocumentAsync(CreateOpenApiDocumentDto document);
    Task<OpenApiDocumentDto?> UpdateDocumentAsync(Guid documentId, UpdateOpenApiDocumentDto document);
    Task DeleteDocumentAsync(Guid documentId);
}
