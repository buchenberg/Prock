using Backend.Core.Domain.Entities.OpenApi;

namespace Backend.Core.Services.Interfaces;

public interface IOpenApiService
{
    Task<IEnumerable<OpenApiSpecification>> GetActiveDocumentsAsync();
    Task<OpenApiSpecification?> GetDocumentByIdAsync(string id);
    Task<OpenApiSpecification?> GetDocumentByTitleAsync(string title);
    Task<OpenApiSpecification> CreateDocumentAsync(OpenApiSpecification document);
    Task<OpenApiSpecification> UpdateDocumentAsync(string id, OpenApiSpecification document);
    Task<bool> DeleteDocumentAsync(string id);
}
