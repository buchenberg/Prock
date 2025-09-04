using Backend.Core.Domain.Entities;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class MockRouteService : IMockRouteService
{
    private readonly ProckDbContext _context;

    public MockRouteService(ProckDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MockRoute>> GetAllAsync()
    {
        return await _context.MockRoutes.ToListAsync();
    }

    public async Task<MockRoute?> GetByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            return null;
        return await _context.MockRoutes.FirstOrDefaultAsync(r => r.RouteId == guidId);
    }

    public async Task<MockRoute> CreateAsync(MockRoute mockRoute)
    {
        mockRoute.RouteId = Guid.NewGuid();
        _context.MockRoutes.Add(mockRoute);
        await _context.SaveChangesAsync();
        return mockRoute;
    }

    public async Task<MockRoute> UpdateAsync(string id, MockRoute mockRoute)
    {
        var existingRoute = await GetByIdAsync(id);
        if (existingRoute == null)
            throw new InvalidOperationException($"MockRoute with ID {id} not found");

        existingRoute.Method = mockRoute.Method;
        existingRoute.Path = mockRoute.Path;
        existingRoute.Mock = mockRoute.Mock;
        existingRoute.HttpStatusCode = mockRoute.HttpStatusCode;
        existingRoute.Enabled = mockRoute.Enabled;

        await _context.SaveChangesAsync();
        return existingRoute;
    }

    public async Task<bool> DisableAsync(string id)
    {
        var route = await GetByIdAsync(id);
        if (route == null) return false;

        route.Enabled = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableAsync(string id)
    {
        var route = await GetByIdAsync(id);
        if (route == null) return false;

        route.Enabled = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var route = await GetByIdAsync(id);
        if (route == null) return false;

        _context.MockRoutes.Remove(route);
        await _context.SaveChangesAsync();
        return true;
    }
}
