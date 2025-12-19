using backend.Data.Dto;

namespace backend.Repositories;

public interface IMockRouteRepository
{
    Task<List<MockRouteDto>> GetAllRoutesAsync();
    Task<MockRouteDto?> GetRouteByIdAsync(Guid routeId);
    Task<MockRouteDto> CreateRouteAsync(MockRouteDto route);
    Task CreateRoutesAsync(List<MockRouteDto> routes);
    Task<MockRouteDto?> UpdateRouteAsync(MockRouteDto route);
    Task<bool> DeleteRouteAsync(Guid routeId);
    Task<int> DeleteAllRoutesAsync();
    Task<MockRouteDto?> SetRouteEnabledAsync(Guid routeId, bool enabled);
    Task<MockRouteDto?> FindMatchingRouteAsync(string path, string method);
}

