using Shared.Contracts.Requests;
using Shared.Contracts.Responses;

namespace Shared.Contracts.Models;

public static class MockRouteMappers
{
    public static MockRouteResponse ToResponse(this MockRouteDto dto)
    {
        return new MockRouteResponse
        {
            RouteId = dto.RouteId.ToString(),
            Method = dto.Method ?? string.Empty,
            Path = dto.Path ?? string.Empty,
            HttpStatusCode = dto.HttpStatusCode,
            Mock = dto.Mock?.ToString(),
            Enabled = dto.Enabled,
            CreatedAt = DateTime.UtcNow, // TODO: Add timestamps to entity
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static MockRouteDto ToDto(this CreateMockRouteRequest request)
    {
        return new MockRouteDto
        {
            RouteId = Guid.NewGuid(), // Will be set by service
            Method = request.Method,
            Path = request.Path,
            HttpStatusCode = request.HttpStatusCode,
            Mock = request.Mock,
            Enabled = request.Enabled
        };
    }

    public static void ApplyUpdate(this MockRouteDto dto, UpdateMockRouteRequest request)
    {
        dto.Method = request.Method;
        dto.Path = request.Path;
        dto.HttpStatusCode = request.HttpStatusCode;
        dto.Mock = request.Mock;
        dto.Enabled = request.Enabled;
    }
}

public static class ConfigMappers
{
    public static ConfigResponse ToResponse(this ProckConfigDto dto, string? host, string? port, string? connectionString)
    {
        return new ConfigResponse
        {
            UpstreamUrl = dto.UpstreamUrl,
            Host = host,
            Port = port,
            ConnectionString = connectionString
        };
    }
}
