using Backend.Core.Domain.Entities;

namespace Backend.Core.Services.Interfaces;

public interface IMockRouteService
{
    Task<IEnumerable<MockRoute>> GetAllAsync();
    Task<MockRoute?> GetByIdAsync(string id);
    Task<MockRoute> CreateAsync(MockRoute mockRoute);
    Task<MockRoute> UpdateAsync(string id, MockRoute mockRoute);
    Task<bool> DisableAsync(string id);
    Task<bool> EnableAsync(string id);
    Task<bool> DeleteAsync(string id);
}
