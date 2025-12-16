using System.Text.Json;
using backend.Services;
using backend.Utils;
using Microsoft.OpenApi.Models;
using Xunit;

namespace backend.UnitTests;

public class MockDataGeneratorTests
{
    private readonly OpenApiDocument _doc;

    public MockDataGeneratorTests()
    {
        _doc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>()
            }
        };
    }

    [Fact]
    public void GenerateMockValue_Integer_ReturnsRandomInteger()
    {
        var schema = new OpenApiSchema { Type = "integer" };
        var random = new Random(12345); // Fixed seed for determinism

        var result = MockDataGenerator.GenerateMockValue(schema, _doc, random);

        Assert.IsType<int>(result);
        var val = (int)result;
        Assert.True(val >= 1 && val < 100);
    }

    [Fact]
    public void GenerateMockValue_String_ReturnsLoremString()
    {
        var schema = new OpenApiSchema { Type = "string" };
        var random = new Random(12345);

        var result = MockDataGenerator.GenerateMockValue(schema, _doc, random);

        Assert.IsType<string>(result);
        Assert.StartsWith("lorem_", (string)result);
    }

    [Fact]
    public void GenerateMockValue_StringEnum_ReturnsEnumValue()
    {
        var schema = new OpenApiSchema
        {
            Type = "string",
            Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>
            {
                new Microsoft.OpenApi.Any.OpenApiString("ChoiceA"),
                new Microsoft.OpenApi.Any.OpenApiString("ChoiceB")
            }
        };
        var random = new Random(12345);

        var result = MockDataGenerator.GenerateMockValue(schema, _doc, random);

        Assert.Contains((string)result, new[] { "ChoiceA", "ChoiceB" });
    }

    [Fact]
    public void GenerateMockValue_Object_ReturnsDictionary()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["prop1"] = new OpenApiSchema { Type = "string" },
                ["prop2"] = new OpenApiSchema { Type = "integer" }
            }
        };
        var random = new Random(12345);

        var result = MockDataGenerator.GenerateMockValue(schema, _doc, random);

        Assert.IsType<Dictionary<string, object?>>(result);
        var dict = (Dictionary<string, object?>)result;
        Assert.True(dict.ContainsKey("prop1"));
        Assert.True(dict.ContainsKey("prop2"));
    }

    [Fact]
    public void GenerateMockValue_Array_ReturnsList()
    {
        var schema = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema { Type = "string" }
        };
        var random = new Random(12345);

        var result = MockDataGenerator.GenerateMockValue(schema, _doc, random);

        Assert.IsType<List<object?>>(result);
        var list = (List<object?>)result;
        Assert.True(list.Count >= 1 && list.Count <= 4);
    }

    [Fact]
    public void GenerateMockValue_Recursion_StopsRecursion()
    {
        // Define a recursive schema: Person -> Friends (Array of Person)
        var personSchema = new OpenApiSchema
        {
            Reference = new OpenApiReference { Id = "Person", Type = ReferenceType.Schema }
        };
        
        var personDef = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new OpenApiSchema { Type = "string" },
                ["friends"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = personSchema // Reference back to Person
                }
            }
        };

        _doc.Components.Schemas["Person"] = personDef;

        var random = new Random(12345);

        // We generate based on the definition, but wrapped in a reference logic usually starts from a reference or top level
        // Let's start with the definition directly, which contains a reference
        var result = MockDataGenerator.GenerateMockValue(personDef, _doc, random);

        Assert.NotNull(result);
        var dict = (Dictionary<string, object?>)result;
        Assert.True(dict.ContainsKey("friends"));
        
        var friends = (List<object?>)dict["friends"];
        // Recursion depth/loop detection should allow 1 level then stop or fall through
        // In our logic: 
        // 1. Root (Person def)
        // 2. friends (Array)
        // 3. Item (Person Ref) -> resolves to Person def. parentRefId passed as "Person".
        // 4. Person def (refId matches "Person"). checks: if (refId == parentRefId) -> fall through.
        // Falls through to object. Properties: name, friends.
        // 5. friends (Array).
        // 6. Item (Person Ref). passes parentRefId = "Person".
        // It should eventually hit depth limit if loop detection doesn't stop it differently or if logical depth increases.
        // Our loop detection prevents immediate infinite loop on the SAME ref.
        
        // Assert that we got something back and didn't crash
        Assert.NotNull(friends);
        Assert.NotEmpty(friends);
    }
}
