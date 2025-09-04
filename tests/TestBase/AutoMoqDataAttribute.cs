using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

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
        fixture.Customize<Backend.Core.Domain.Entities.MariaDb.MockRoute>(composer =>
            composer
                .With(x => x.RouteId, () => Guid.NewGuid().ToString())
                .With(x => x.Method, () => new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }[new Random().Next(5)])
                .With(x => x.HttpStatusCode, () => 200)
                .With(x => x.Enabled, () => true)
                .With(x => x.Path, () => "/api/test")
                .With(x => x.Mock, () => JsonSerializer.Serialize(new { message = "test" })));

        fixture.Customize<Backend.Core.Domain.Entities.MariaDb.ProckConfig>(composer =>
            composer
                .With(x => x.UpstreamUrl, () => "https://api.example.com")
                .With(x => x.Host, () => "localhost")
                .With(x => x.Port, () => "5001"));

        fixture.Customize<Backend.Core.Domain.Entities.MariaDb.OpenApiSpecification>(composer =>
            composer
                .With(x => x.Title, () => "Test API")
                .With(x => x.Description, () => "Test API Description")
                .With(x => x.Version, () => "1.0.0")
                .With(x => x.OpenApiVersion, () => "3.0.0")
                .With(x => x.Content, () => JsonSerializer.Serialize(new { openapi = "3.0.0", info = new { title = "Test API", version = "1.0.0" } }))
                .With(x => x.CreatedAt, () => DateTime.UtcNow.AddDays(-1))
                .With(x => x.UpdatedAt, () => DateTime.UtcNow)
                .With(x => x.IsActive, () => true));

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
