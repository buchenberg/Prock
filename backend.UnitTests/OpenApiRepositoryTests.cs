using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Xunit;

namespace backend.UnitTests;

public class OpenApiRepositoryTests
{
    private readonly ProckDbContext _context;
    private readonly OpenApiRepository _repository;

    public OpenApiRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProckDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProckDbContext(options);
        _repository = new OpenApiRepository(_context);
    }

    [Fact]
    public async Task CreateDocumentAsync_AddsDocumentAndReturnsDto()
    {
        var json = "{\"openapi\":\"3.0.0\",\"info\":{\"title\":\"Test API\",\"version\":\"1.0.0\"},\"paths\":{}}";
        var request = new CreateOpenApiDocumentDto { OpenApiJson = json };

        var result = await _repository.CreateDocumentAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Test API", result.Title);

        var savedDoc = await _context.OpenApiDocuments.FirstOrDefaultAsync(d => d.DocumentId == result.DocumentId);
        Assert.NotNull(savedDoc);
        Assert.Equal("Test API", savedDoc.Title);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ReturnsDto_WhenExists()
    {
        var doc = new OpenApiSpecification { _id = ObjectId.GenerateNewId(), DocumentId = Guid.NewGuid(), Title = "Test Doc", IsActive = true };
        _context.OpenApiDocuments.Add(doc);
        await _context.SaveChangesAsync();

        var retrieved = await _repository.GetDocumentByIdAsync(doc.DocumentId);

        Assert.NotNull(retrieved);
        Assert.Equal(doc.DocumentId, retrieved.DocumentId);
        Assert.IsType<OpenApiDocumentDetailDto>(retrieved);
    }

    [Fact]
    public async Task GetAllDocumentsAsync_ReturnsDtos()
    {
        var doc1 = new OpenApiSpecification { _id = ObjectId.GenerateNewId(), DocumentId = Guid.NewGuid(), Title = "Doc 1", IsActive = true };
        _context.OpenApiDocuments.Add(doc1);
        await _context.SaveChangesAsync();

        var docs = await _repository.GetAllDocumentsAsync();

        Assert.Single(docs);
        Assert.Equal("Doc 1", docs[0].Title);
        Assert.IsType<OpenApiDocumentDto>(docs[0]);
    }

    [Fact]
    public async Task UpdateDocumentAsync_UpdatesAttributes()
    {
        var doc = new OpenApiSpecification { _id = ObjectId.GenerateNewId(), DocumentId = Guid.NewGuid(), Title = "Old Title", IsActive = true };
        _context.OpenApiDocuments.Add(doc);
        await _context.SaveChangesAsync();

        var update = new UpdateOpenApiDocumentDto { Title = "New Title" };
        var result = await _repository.UpdateDocumentAsync(doc.DocumentId, update);

        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);

        var saved = await _context.OpenApiDocuments.FirstOrDefaultAsync(d => d.DocumentId == doc.DocumentId);
        Assert.Equal("New Title", saved.Title);
    }

    [Fact]
    public async Task DeleteDocumentAsync_DeletesDocument()
    {
        var doc = new OpenApiSpecification { _id = ObjectId.GenerateNewId(), DocumentId = Guid.NewGuid(), Title = "To Delete", IsActive = true };
        _context.OpenApiDocuments.Add(doc);
        await _context.SaveChangesAsync();

        await _repository.DeleteDocumentAsync(doc.DocumentId);

        var savedDoc = await _context.OpenApiDocuments.FirstOrDefaultAsync(d => d.DocumentId == doc.DocumentId);
        Assert.Null(savedDoc);
    }
}
