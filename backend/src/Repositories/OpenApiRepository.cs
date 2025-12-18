using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using backend.Utils;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class OpenApiRepository : IOpenApiRepository
{
    private readonly ProckDbContext _db;

    public OpenApiRepository(ProckDbContext db)
    {
        _db = db;
    }

    public async Task<List<OpenApiDocumentDto>> GetAllDocumentsAsync()
    {
        var entities = await _db.OpenApiDocuments
            .Where(x => x.IsActive)
            .ToListAsync();

        return entities.Select(doc => new OpenApiDocumentDto
        {
            DocumentId = doc.DocumentId,
            Title = doc.Title,
            Version = doc.Version,
            Description = doc.Description,
            OpenApiVersion = doc.OpenApiVersion,
            BasePath = doc.BasePath,
            Host = doc.Host,
            Schemes = doc.Schemes,
            Consumes = doc.Consumes,
            Produces = doc.Produces,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt,
            IsActive = doc.IsActive
        }).ToList();
    }

    public async Task<OpenApiDocumentDetailDto?> GetDocumentByIdAsync(Guid documentId)
    {
        var document = await _db.OpenApiDocuments
            .Where(x => x.DocumentId == documentId)
            .FirstOrDefaultAsync();

        if (document == null) return null;

        return new OpenApiDocumentDetailDto
        {
            DocumentId = document.DocumentId,
            Title = document.Title,
            Version = document.Version,
            Description = document.Description,
            OpenApiVersion = document.OpenApiVersion,
            BasePath = document.BasePath,
            Host = document.Host,
            Schemes = document.Schemes,
            Consumes = document.Consumes,
            Produces = document.Produces,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsActive = document.IsActive,
            OriginalJson = document.OriginalJson
        };
    }

    public async Task<OpenApiDocumentDto> CreateDocumentAsync(CreateOpenApiDocumentDto request)
    {
        var msOpenApiDocument = MockDataGenerator.ParseOpenApiSpec(request.OpenApiJson) 
                                ?? throw new ArgumentException("Invalid OpenAPI Specification");

        var oasDocumentId = Guid.NewGuid();
        var entity = new OpenApiSpecification
        {
            DocumentId = oasDocumentId,
            Title = request.Title ?? msOpenApiDocument.Info?.Title ?? "Untitled API",
            Version = request.Version ?? msOpenApiDocument.Info?.Version ?? "1.0.0",
            Description = request.Description ?? msOpenApiDocument.Info?.Description,
            OpenApiVersion = "3.0.0",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            OriginalJson = request.OpenApiJson,
            Paths = msOpenApiDocument.Paths.Select(path => new OpenApiPath
            {
                Path = path.Key,
                Description = path.Value.Description,
                Summary = path.Value.Summary,
            }).ToList(),
        };

        _db.OpenApiDocuments.Add(entity);
        await _db.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<OpenApiDocumentDto?> UpdateDocumentAsync(Guid documentId, UpdateOpenApiDocumentDto request)
    {
        var entity = await _db.GetOpenApiDocumentByIdAsync(documentId);
        if (entity == null) return null;

        if (!string.IsNullOrEmpty(request.OpenApiJson))
        {
             var msOpenApiDocument = MockDataGenerator.ParseOpenApiSpec(request.OpenApiJson) 
                                     ?? throw new ArgumentException("Invalid OpenAPI Specification");
             entity.OriginalJson = request.OpenApiJson;
             // Update derived fields if Json changes? Usually paths changes.
             // EF Core owned collection replacement:
             entity.Paths = msOpenApiDocument.Paths.Select(path => new OpenApiPath
             {
                 Path = path.Key,
                 Description = path.Value.Description,
                 Summary = path.Value.Summary,
             }).ToList();
        }

        entity.Title = request.Title ?? entity.Title;
        entity.Version = request.Version ?? entity.Version;
        entity.Description = request.Description ?? entity.Description;
        entity.IsActive = request.IsActive ?? entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _db.OpenApiDocuments.Update(entity);
        await _db.SaveChangesAsync();
        
        return MapToDto(entity);
    }

    private static OpenApiDocumentDto MapToDto(OpenApiSpecification entity)
    {
        return new OpenApiDocumentDto
        {
            DocumentId = entity.DocumentId,
            Title = entity.Title,
            Version = entity.Version,
            Description = entity.Description,
            OpenApiVersion = entity.OpenApiVersion,
            BasePath = entity.BasePath,
            Host = entity.Host,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive
        };
    }



    public async Task DeleteDocumentAsync(Guid documentId)
    {
        var document = await _db.GetOpenApiDocumentByIdAsync(documentId);
        if (document != null)
        {
            _db.OpenApiDocuments.Remove(document);
            await _db.SaveChangesAsync();
        }
    }
}
