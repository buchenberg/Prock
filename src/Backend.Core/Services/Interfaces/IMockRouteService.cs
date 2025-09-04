using Backend.Core.Domain.Entities.MariaDb;

namespace Backend.Core.Services.Interfaces;

public interface IMockRouteService
{
    Task<IEnumerable<MockRoute>> GetAllAsync();
    Task<MockRoute?> GetByIdAsync(int id);
    Task<MockRoute?> GetByRouteIdAsync(string routeId);
    Task<MockRoute> CreateAsync(MockRoute mockRoute);
    Task<MockRoute> UpdateAsync(int id, MockRoute mockRoute);
    Task<bool> DisableAsync(int id);
    Task<bool> EnableAsync(int id);
    Task<bool> DeleteAsync(int id);
}
