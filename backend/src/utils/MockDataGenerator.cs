using System.Text.Json;
using Microsoft.OpenApi.Readers;
using MsOpenApi = Microsoft.OpenApi.Models;

namespace backend.Utils;

public static class MockDataGenerator
{
    public static MsOpenApi.OpenApiDocument? ParseOpenApiSpec(string specContent)
    {
        Console.WriteLine($"OpenAPI document parse attempt for content of length {specContent.Length}");
        try
        {
            var reader = new OpenApiStringReader(settings: new OpenApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences,
                // LoadExternalRefs = false,
            });

            var openApiDocument = reader.Read(specContent, out var diagnostic);
            
            if (diagnostic.Errors.Count > 0)
            {
                // Log errors but don't fail if we got a document. Many tools produce "invalid" but usable specs (e.g. .NET generics in schema names).
                Console.WriteLine($"OpenAPI document parsing warnings: {string.Join(", ", diagnostic.Errors.Select(e => e.Message))}");
            }

            if (openApiDocument != null)
            {
                 Console.WriteLine($"OpenAPI document parse attempt success!");
                 return openApiDocument;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing OpenAPI JSON: {ex.Message}");
            return null;
        }
    }

    public static object? GenerateMockValue(MsOpenApi.OpenApiSchema schema, MsOpenApi.OpenApiDocument doc, Random random, int depth = 0, string? parentRefId = null)
    {
        if (depth > 5) return null; 

        if (schema.Reference != null)
        {
            var refId = schema.Reference.Id;
            
            // Handle self-reference / recursion prevention
            if (refId == parentRefId)
            {
                // Fall through to process the schema structure itself
                // Or stop if it's a direct cycle that we can't break easily without structural knowledge
                // For now, let's try to fall through, but if the schema itself DOES NOT have type info (just a ref), we return null/default
            }
            else
            {
                if (doc.Components != null && doc.Components.Schemas.TryGetValue(refId, out var refSchema))
                {
                    // Pass the CURRENT refId as parentRefId to the child to detect loops
                    return GenerateMockValue(refSchema, doc, random, depth + 1, refId);
                }
            }
        }

        switch (schema.Type)
        {
            case "string":
                 if (schema.Enum != null && schema.Enum.Count > 0)
                {
                    var index = random.Next(schema.Enum.Count);
                    // OpenApiAny is the base type, we need to extract value. 
                     // For simplicity in this mock gen, we'll TryToString or cast if appropriate.
                     // The library might return IOpenApiPrimitive
                     var enumVal = schema.Enum[index];
                     if (enumVal is Microsoft.OpenApi.Any.OpenApiString s) return s.Value;
                     return "enum_val";
                }
                if (schema.Format == "date-time") return DateTime.UtcNow.ToString("o");
                if (schema.Format == "uuid") return Guid.NewGuid().ToString();
                return $"lorem_{random.Next(1000)}";

            case "integer":
                return random.Next(1, 100);

            case "number":
                return random.NextDouble() * 100.0;

            case "boolean":
                return random.Next(2) == 0;

            case "array":
                 if (schema.Items != null)
                {
                    var count = random.Next(1, 4); // generate 1-3 items
                    var list = new List<object?>();
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(GenerateMockValue(schema.Items, doc, random, depth + 1, parentRefId));
                    }
                    return list;
                }
                return new List<object>();

            case "object":
                 if (schema.Properties != null && schema.Properties.Count > 0)
                {
                    var obj = new Dictionary<string, object?>();
                    foreach (var prop in schema.Properties)
                    {
                        obj[prop.Key] = GenerateMockValue(prop.Value, doc, random, depth + 1, parentRefId);
                    }
                    return obj;
                }
                return new Dictionary<string, object>();

            default:
                if (schema.Properties != null && schema.Properties.Count > 0)
                {
                    var implicitObj = new Dictionary<string, object?>();
                    foreach (var prop in schema.Properties)
                    {
                         implicitObj[prop.Key] = GenerateMockValue(prop.Value, doc, random, depth + 1, parentRefId);
                    }
                    return implicitObj;
                }
                return null;
        }
    }
}
