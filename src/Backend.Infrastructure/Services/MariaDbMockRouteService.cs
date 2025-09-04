using Backend.Core.Domain.Entities.MariaDb;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class MockRouteService : IMockRouteService
{
    private readonly MariaDbContext _context;

    public MockRouteService(MariaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MockRoute>> GetAllAsync()
    {
        return await _context.MockRoutes.ToListAsync();
    }

    public async Task<MockRoute?> GetByIdAsync(int id)
    {
        return await _context.MockRoutes.FindAsync(id);
    }

    public async Task<MockRoute> UpdateAsync(int id, MockRoute mockRoute)
    {
        var existingRoute = await _context.MockRoutes.FindAsync(id);
        if (existingRoute == null)
            throw new ArgumentException($"MockRoute with ID {id} not found");

        // Update properties
        existingRoute.Method = mockRoute.Method;
        existingRoute.Path = mockRoute.Path;
        existingRoute.HttpStatusCode = mockRoute.HttpStatusCode;
        existingRoute.Mock = mockRoute.Mock;
        existingRoute.Enabled = mockRoute.Enabled;

        _context.MockRoutes.Update(existingRoute);
        await _context.SaveChangesAsync();
        
        return existingRoute;
    }

    public async Task<MockRoute?> GetByRouteIdAsync(string routeId)
    {
        return await _context.MockRoutes
            .FirstOrDefaultAsync(mr => mr.RouteId == routeId);
    }

    public async Task<MockRoute> CreateAsync(MockRoute mockRoute)
    {
        // Check for duplicate route (same path + method)
        var existingRoute = await _context.MockRoutes
            .FirstOrDefaultAsync(mr => 
                mr.Path == mockRoute.Path && 
                mr.Method == mockRoute.Method);
        
        if (existingRoute != null)
        {
            throw new InvalidOperationException($"A mock route already exists for {mockRoute.Method} {mockRoute.Path}");
        }
        
        _context.MockRoutes.Add(mockRoute);
        await _context.SaveChangesAsync();
        return mockRoute;
    }

    public async Task<MockRoute> UpdateAsync(MockRoute mockRoute)
    {
        _context.MockRoutes.Update(mockRoute);
        await _context.SaveChangesAsync();
        return mockRoute;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var mockRoute = await _context.MockRoutes.FindAsync(id);
        if (mockRoute == null) return false;

        _context.MockRoutes.Remove(mockRoute);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableAsync(int id)
    {
        var mockRoute = await _context.MockRoutes.FindAsync(id);
        if (mockRoute == null) return false;

        mockRoute.Enabled = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableAsync(int id)
    {
        var mockRoute = await _context.MockRoutes.FindAsync(id);
        if (mockRoute == null) return false;

        mockRoute.Enabled = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
