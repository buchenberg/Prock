using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using MongoDB.Bson;

namespace backend.Tests.TestBase;

/// <summary>
/// Custom AutoData attribute that automatically configures AutoFixture with AutoMoq
/// and sets up common customizations for our domain objects.
/// </summary>
public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(() => CreateFixture())
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        
        // Configure AutoMoq
        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
        
        // Add common customizations
        fixture.Customize<backend.Data.Entities.MockRoute>(composer =>
            composer
                .With(x => x.RouteId, () => Guid.NewGuid())
                .With(x => x.Method, () => new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }[new Random().Next(5)])
                .With(x => x.HttpStatusCode, () => 200)
                .With(x => x.Enabled, true)
                .With(x => x.Path, () => "/api/test")
                .With(x => x.Mock, () => JsonSerializer.Serialize(new { message = "test" })));

        fixture.Customize<backend.Data.Entities.ProckConfig>(composer =>
            composer
                .With(x => x.Id, () => Guid.NewGuid())
                .With(x => x.UpstreamUrl, () => "https://api.example.com"));

        fixture.Customize<backend.Data.Entities.OpenApiSpecification>(composer =>
            composer
                .With(x => x.DocumentId, () => Guid.NewGuid())
                .With(x => x.CreatedAt, () => DateTime.UtcNow.AddDays(-1))
                .With(x => x.UpdatedAt, () => DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .With(x => x.OpenApiVersion, "3.0.0"));

        // Fix ObjectId generation for MongoDB entities
        fixture.Customize<ObjectId>(composer =>
            composer.FromFactory(() => ObjectId.GenerateNewId()));

        return fixture;
    }
}

/// <summary>
/// Inline version of AutoMoqDataAttribute for parameterized tests
/// </summary>
public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values) : base(new AutoMoqDataAttribute(), values)
    {
    }
}
