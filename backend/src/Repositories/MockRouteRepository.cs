using System.Text.Json;
using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace backend.Repositories;

public class MockRouteRepository : IMockRouteRepository
{
    private readonly ProckDbContext _db;

    public MockRouteRepository(ProckDbContext db)
    {
        _db = db;
    }

    public async Task<List<MockRouteDto>> GetAllRoutesAsync()
    {
        var entities = await _db.MockRoutes.ToListAsync();
        
        return entities.Select(x => new MockRouteDto
        {
            RouteId = x.RouteId,
            Method = x.Method,
            Path = x.Path,
            HttpStatusCode = x.HttpStatusCode,
            Mock = x.Mock != null ? JsonSerializer.Deserialize<dynamic>(x.Mock, (JsonSerializerOptions?)null) : null,
            Enabled = x.Enabled
        }).ToList();
    }

    public async Task<MockRouteDto?> GetRouteByIdAsync(Guid routeId)
    {
        var route = await _db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeId);
        return route == null ? null : MapToDto(route);
    }

    public async Task<MockRouteDto> CreateRouteAsync(MockRouteDto routeDto)
    {
        var entity = new MockRoute
        {
            _id = ObjectId.GenerateNewId(),
            RouteId = Guid.NewGuid(), // Ensure valid GUID
            Method = (routeDto.Method ?? string.Empty).ToUpper(),
            Path = routeDto.Path,
            HttpStatusCode = routeDto.HttpStatusCode,
            Mock = JsonSerializer.Serialize<object>(routeDto.Mock),
            Enabled = true
        };

        _db.MockRoutes.Add(entity);
        await _db.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task CreateRoutesAsync(List<MockRouteDto> routes)
    {
        var entities = routes.Select(routeDto => new MockRoute
        {
            _id = ObjectId.GenerateNewId(),
            RouteId = routeDto.RouteId != Guid.Empty ? routeDto.RouteId : Guid.NewGuid(),
            Method = (routeDto.Method ?? string.Empty).ToUpper(),
            Path = routeDto.Path,
            HttpStatusCode = routeDto.HttpStatusCode,
            Mock = JsonSerializer.Serialize<object>(routeDto.Mock),
            Enabled = true
        }).ToList();

        _db.MockRoutes.AddRange(entities);
        await _db.SaveChangesAsync();
    }

    public async Task<MockRouteDto?> UpdateRouteAsync(MockRouteDto routeDto)
    {
        var entity = await _db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeDto.RouteId);
        if (entity == null) return null;

        entity.Path = routeDto.Path;
        entity.Method = (routeDto.Method ?? string.Empty).ToUpper();
        entity.HttpStatusCode = routeDto.HttpStatusCode;
        entity.Mock = JsonSerializer.Serialize<object>(routeDto.Mock);

        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> DeleteRouteAsync(Guid routeId)
    {
        var entity = await _db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeId);
        if (entity == null) return false;

        _db.MockRoutes.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<MockRouteDto?> SetRouteEnabledAsync(Guid routeId, bool enabled)
    {
        var entity = await _db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeId);
        if (entity == null) return null;

        entity.Enabled = enabled;
        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<MockRouteDto?> FindMatchingRouteAsync(string path, string method)
    {
        var entity = await _db.MockRoutes.SingleOrDefaultAsync(x =>
            string.Equals(x.Path, path, StringComparison.CurrentCultureIgnoreCase) &&
            string.Equals(x.Method, method, StringComparison.CurrentCultureIgnoreCase) &&
            x.Enabled);

        return entity == null ? null : MapToDto(entity);
    }

    private static MockRouteDto MapToDto(MockRoute entity)
    {
        return new MockRouteDto
        {
            RouteId = entity.RouteId,
            Method = entity.Method,
            Path = entity.Path,
            HttpStatusCode = entity.HttpStatusCode,
            Mock = entity.Mock != null ? JsonSerializer.Deserialize<dynamic>(entity.Mock, (JsonSerializerOptions?)null) : null,
            Enabled = entity.Enabled
        };
    }
}
