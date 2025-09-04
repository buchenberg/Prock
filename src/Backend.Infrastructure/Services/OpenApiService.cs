using Backend.Core.Domain.Entities.OpenApi;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class OpenApiService : IOpenApiService
{
    private readonly ProckDbContext _context;

    public OpenApiService(ProckDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OpenApiSpecification>> GetActiveDocumentsAsync()
    {
        return await _context.OpenApiDocuments
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<OpenApiSpecification?> GetDocumentByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            return null;
            
        return await _context.OpenApiDocuments
            .FirstOrDefaultAsync(x => x.DocumentId == guidId);
    }

    public async Task<OpenApiSpecification?> GetDocumentByTitleAsync(string title)
    {
        return await _context.OpenApiDocuments
            .Where(x => x.IsActive)
            .FirstOrDefaultAsync(x => x.Title == title);
    }

    public async Task<OpenApiSpecification> CreateDocumentAsync(OpenApiSpecification document)
    {
        document.DocumentId = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;
        
        _context.OpenApiDocuments.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<OpenApiSpecification> UpdateDocumentAsync(string id, OpenApiSpecification document)
    {
        var existingDocument = await GetDocumentByIdAsync(id);
        if (existingDocument == null)
            throw new InvalidOperationException($"OpenAPI document with ID {id} not found");

        existingDocument.Title = document.Title;
        existingDocument.Version = document.Version;
        existingDocument.Description = document.Description;
        existingDocument.OriginalJson = document.OriginalJson;
        existingDocument.IsActive = document.IsActive;
        existingDocument.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingDocument;
    }

    public async Task<bool> DeleteDocumentAsync(string id)
    {
        var document = await GetDocumentByIdAsync(id);
        if (document == null) return false;

        _context.OpenApiDocuments.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }
}
