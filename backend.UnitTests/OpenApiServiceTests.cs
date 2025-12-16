using backend.Data.Dto;
using backend.Data.Entities;
using backend.Endpoints;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using Moq;
using Xunit;

namespace backend.UnitTests;

public class OpenApiServiceTests
{
    private readonly Mock<IOpenApiRepository> _mockRepo;
    private readonly Mock<IMockRouteRepository> _mockRouteRepo;
    private readonly OpenApiService _service;

    public OpenApiServiceTests()
    {
        _mockRepo = new Mock<IOpenApiRepository>();
        _mockRouteRepo = new Mock<IMockRouteRepository>();
        _service = new OpenApiService(_mockRepo.Object, _mockRouteRepo.Object);
    }

    [Fact]
    public async Task CreateDocumentAsync_ValidJson_CreatesDocument()
    {
        var request = new CreateOpenApiDocumentDto 
        { 
            OpenApiJson = "{\"openapi\":\"3.0.0\",\"info\":{\"title\":\"Test API\",\"version\":\"1.0.0\"},\"paths\":{}}" 
        };

        var expectedDto = new OpenApiDocumentDto { DocumentId = Guid.NewGuid(), Title = "Test API" };

        _mockRepo.Setup(r => r.CreateDocumentAsync(It.IsAny<CreateOpenApiDocumentDto>()))
                 .ReturnsAsync(expectedDto);

        var result = await _service.CreateDocumentAsync(request);

        Assert.IsType<Created<OpenApiDocumentDto>>(result.Result);
        _mockRepo.Verify(r => r.CreateDocumentAsync(It.Is<CreateOpenApiDocumentDto>(d => d.OpenApiJson == request.OpenApiJson)), Times.Once);
    }

    [Fact]
    public async Task CreateDocumentAsync_InvalidJson_ReturnsBadRequest()
    {
        // Repo is responsible for parsing now. Mock it throwing ArgumentException?
        var request = new CreateOpenApiDocumentDto { OpenApiJson = "Invalid JSON" };
        
        _mockRepo.Setup(r => r.CreateDocumentAsync(It.IsAny<CreateOpenApiDocumentDto>()))
                 .ThrowsAsync(new ArgumentException("Invalid OpenAPI JSON"));

        var result = await _service.CreateDocumentAsync(request);

        Assert.IsType<BadRequest<string>>(result.Result);
    }

    [Fact]
    public async Task GenerateMockRoutesAsync_DocumentNotFound_ReturnsNotFound()
    {
        var docId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetDocumentByIdAsync(docId)).ReturnsAsync((OpenApiDocumentDetailDto?)null);

        var result = await _service.GenerateMockRoutesAsync(docId);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GenerateMockRoutesAsync_ValidDocument_GeneratesRoutes()
    {
        var docId = Guid.NewGuid();
        var json = "{\"openapi\":\"3.0.0\",\"info\":{\"title\":\"Test API\",\"version\":\"1.0.0\"},\"paths\":{\"/pets\":{\"get\":{\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}}}}}}}}}";
        var doc = new OpenApiDocumentDetailDto { DocumentId = docId, OriginalJson = json };

        _mockRepo.Setup(r => r.GetDocumentByIdAsync(docId)).ReturnsAsync(doc);
        _mockRouteRepo.Setup(r => r.CreateRoutesAsync(It.IsAny<List<MockRouteDto>>())).Returns(Task.CompletedTask);

        var result = await _service.GenerateMockRoutesAsync(docId);

        var okResult = Assert.IsType<Ok<List<MockRouteDto>>>(result.Result);
        Assert.NotNull(okResult.Value);
        Assert.Single(okResult.Value);
        Assert.Equal("/pets", okResult.Value[0].Path);
        Assert.Equal("GET", okResult.Value[0].Method);
        
        _mockRouteRepo.Verify(r => r.CreateRoutesAsync(It.Is<List<MockRouteDto>>(l => l.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_DocumentExists_Deletes()
    {
        var docId = Guid.NewGuid();
        var doc = new OpenApiDocumentDetailDto { DocumentId = docId, IsActive = true };

        _mockRepo.Setup(r => r.GetDocumentByIdAsync(docId)).ReturnsAsync(doc);
        _mockRepo.Setup(r => r.DeleteDocumentAsync(docId)).Returns(Task.CompletedTask);

        var result = await _service.DeleteDocumentAsync(docId);

        Assert.IsType<NoContent>(result.Result);
        _mockRepo.Verify(r => r.DeleteDocumentAsync(docId), Times.Once);
    }
}
