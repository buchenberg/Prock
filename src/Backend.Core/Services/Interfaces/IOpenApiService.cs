using Backend.Core.Domain.Entities.MariaDb;

namespace Backend.Core.Services.Interfaces;

public interface IOpenApiService
{
    Task<IEnumerable<OpenApiSpecification>> GetAllAsync();
    Task<IEnumerable<OpenApiSpecification>> GetActiveAsync();
    Task<OpenApiSpecification?> GetByIdAsync(int id);
    Task<OpenApiSpecification?> GetByTitleAsync(string title);
    Task<OpenApiSpecification> CreateAsync(OpenApiSpecification specification);
    Task<OpenApiSpecification> UpdateAsync(int id, OpenApiSpecification specification);
    Task<bool> DeleteAsync(int id);
    Task<bool> ActivateAsync(int id);
    Task<bool> DeactivateAsync(int id);
}
