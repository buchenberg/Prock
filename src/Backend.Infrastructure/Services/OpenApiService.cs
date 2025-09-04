using Backend.Core.Domain.Entities.MariaDb;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class OpenApiService : IOpenApiService
{
    private readonly MariaDbContext _context;

    public OpenApiService(MariaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OpenApiSpecification>> GetAllAsync()
    {
        return await _context.OpenApiSpecifications.ToListAsync();
    }

    public async Task<IEnumerable<OpenApiSpecification>> GetActiveAsync()
    {
        return await _context.OpenApiSpecifications
            .Where(spec => spec.IsActive)
            .ToListAsync();
    }

    public async Task<OpenApiSpecification?> GetByIdAsync(int id)
    {
        return await _context.OpenApiSpecifications.FindAsync(id);
    }

    public async Task<OpenApiSpecification?> GetByTitleAsync(string title)
    {
        return await _context.OpenApiSpecifications
            .FirstOrDefaultAsync(spec => spec.Title == title && spec.IsActive);
    }

    public async Task<OpenApiSpecification> CreateAsync(OpenApiSpecification specification)
    {
        _context.OpenApiSpecifications.Add(specification);
        await _context.SaveChangesAsync();
        return specification;
    }

    public async Task<OpenApiSpecification> UpdateAsync(int id, OpenApiSpecification specification)
    {
        var existingSpec = await _context.OpenApiSpecifications.FindAsync(id);
        if (existingSpec == null)
            throw new ArgumentException($"OpenAPI specification with ID {id} not found");

        // Update properties
        existingSpec.Title = specification.Title;
        existingSpec.Description = specification.Description;
        existingSpec.Version = specification.Version;
        existingSpec.OpenApiVersion = specification.OpenApiVersion;
        existingSpec.Content = specification.Content;
        existingSpec.IsActive = specification.IsActive;
        existingSpec.UpdatedAt = DateTime.UtcNow;

        _context.OpenApiSpecifications.Update(existingSpec);
        await _context.SaveChangesAsync();
        return existingSpec;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var spec = await _context.OpenApiSpecifications.FindAsync(id);
        if (spec == null)
            return false;

        _context.OpenApiSpecifications.Remove(spec);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateAsync(int id)
    {
        var spec = await _context.OpenApiSpecifications.FindAsync(id);
        if (spec == null)
            return false;

        spec.IsActive = true;
        spec.UpdatedAt = DateTime.UtcNow;
        _context.OpenApiSpecifications.Update(spec);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var spec = await _context.OpenApiSpecifications.FindAsync(id);
        if (spec == null)
            return false;

        spec.IsActive = false;
        spec.UpdatedAt = DateTime.UtcNow;
        _context.OpenApiSpecifications.Update(spec);
        await _context.SaveChangesAsync();
        return true;
    }
}


