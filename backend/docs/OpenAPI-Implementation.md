# OpenAPI Document Management

This implementation adds comprehensive OpenAPI document storage and management capabilities to your Prock application using MongoDB and EF Core.

## Features

### Entity Classes
- **OpenApiDocument**: Main entity storing complete OpenAPI specifications
- **OpenApiPath**: Path and operation definitions
- **OpenApiSchema**: Schema definitions with full JSON Schema support
- **OpenApiComponents**: Components including schemas, responses, parameters, etc.
- **Supporting entities**: Server, Tag, Security, etc.

### API Endpoints

#### GET `/prock/api/openapi-documents`
Returns all active OpenAPI documents with basic information.

#### GET `/prock/api/openapi-documents/{documentId}`
Returns detailed information for a specific OpenAPI document including paths, operations, and metadata.

#### POST `/prock/api/openapi-documents`
Creates a new OpenAPI document from JSON specification.

**Request Body:**
```json
{
  "title": "My API",
  "version": "1.0.0", 
  "description": "API description",
  "openApiJson": "{ ... full OpenAPI 3.0 JSON ... }"
}
```

#### PUT `/prock/api/openapi-documents/{documentId}`
Updates an existing OpenAPI document.

#### DELETE `/prock/api/openapi-documents/{documentId}`
Soft deletes an OpenAPI document (sets IsActive to false).

#### GET `/prock/api/openapi-documents/{documentId}/json`
Returns the original OpenAPI JSON specification.

### Database Schema

The entities are stored in MongoDB collections:
- `openApiDocuments`: Main document collection
- Documents include nested objects for paths, operations, schemas, etc.

### Usage Examples

1. **Store an OpenAPI document:**
   - POST to `/prock/api/openapi-documents` with OpenAPI JSON
   - The system parses and extracts structured information
   - Provides search and filtering capabilities

2. **Query documents:**
   - Get all documents to see available APIs
   - Get specific document for detailed path/operation info
   - Filter by active status

3. **Integration with EF Core:**
   ```csharp
   // Get all active documents
   var docs = await dbContext.GetActiveOpenApiDocumentsAsync();
   
   // Get specific document
   var doc = await dbContext.GetOpenApiDocumentByIdAsync(documentId);
   
   // Get by title
   var doc = await dbContext.GetOpenApiDocumentByTitleAsync("My API");
   ```

### Benefits

- **Structured Storage**: OpenAPI specs stored as structured data, not just text
- **Queryable**: Use EF Core LINQ to query paths, operations, schemas
- **Versioning**: Track creation/update times, soft delete support
- **Validation**: Automatic OpenAPI JSON validation on input
- **Rich Metadata**: Extract and store servers, tags, security schemes, etc.

This implementation provides a foundation for building API documentation systems, mock servers, testing tools, and other OpenAPI-powered features.
