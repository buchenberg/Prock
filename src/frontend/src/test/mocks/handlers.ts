import { http, HttpResponse } from 'msw'

// Mock data - make a fresh copy for each test
const getInitialMockRoutes = () => [
  {
    id: 1,
    routeId: 'route-1',
    method: 'GET',
    path: '/api/test',
    httpStatusCode: 200,
    enabled: true,
    mock: { message: 'Hello World' }
  },
  {
    id: 2,
    routeId: 'route-2',
    method: 'POST',
    path: '/api/users',
    httpStatusCode: 201,
    enabled: false,
    mock: { id: 1, name: 'John Doe' }
  }
]

let mockRoutes = getInitialMockRoutes()

const getInitialOpenApiDocuments = () => [
  {
    documentId: '1',
    title: 'Test API',
    version: '1.0.0',
    description: 'Test API Description',
    openApiVersion: '3.0.1',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    isActive: true,
    pathCount: 5
  },
  {
    documentId: '2',
    title: 'User API',
    version: '2.0.0',
    description: 'User management API',
    openApiVersion: '3.0.1',
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
    isActive: false,
    pathCount: 3
  }
]

let mockOpenApiDocuments = getInitialOpenApiDocuments()

// Reset function for tests
export const resetMockData = () => {
  mockRoutes = getInitialMockRoutes()
  mockOpenApiDocuments = getInitialOpenApiDocuments()
}

const mockConfig = {
  connectionString: 'test-connection',
  upstreamUrl: 'http://localhost:3000',
  host: 'localhost',
  port: '5001'
}

const mockStatusCodes = [
  { "200": "OK" },
  { "201": "Created" },
  { "400": "Bad Request" },
  { "404": "Not Found" },
  { "500": "Internal Server Error" }
]

const mockContentTypes = [
  'application/json',
  'application/xml',
  'text/plain',
  'text/html'
]

export const handlers = [
  // Mock Routes API
  http.get('/prock/api/mock-routes', () => {
    return HttpResponse.json(mockRoutes)
  }),

  http.post('/prock/api/mock-routes', async ({ request }) => {
    const newRoute = await request.json() as any
    const createdRoute = {
      id: mockRoutes.length + 1,
      routeId: `route-${mockRoutes.length + 1}`,
      ...newRoute
    }
    mockRoutes.push(createdRoute)
    return HttpResponse.json(createdRoute, { status: 201 })
  }),

  http.put('/prock/api/mock-routes/:id', async ({ params, request }) => {
    const id = parseInt(params.id as string)
    const updatedRoute = await request.json() as any
    const routeIndex = mockRoutes.findIndex(r => r.id === id)
    
    if (routeIndex === -1) {
      return HttpResponse.json({ message: 'Route not found' }, { status: 404 })
    }
    
    mockRoutes[routeIndex] = { ...mockRoutes[routeIndex], ...updatedRoute }
    return HttpResponse.json(mockRoutes[routeIndex])
  }),

  http.delete('/prock/api/mock-routes/:id', ({ params }) => {
    const id = parseInt(params.id as string)
    const routeIndex = mockRoutes.findIndex(r => r.id === id)
    
    if (routeIndex === -1) {
      return HttpResponse.json({ message: 'Route not found' }, { status: 404 })
    }
    
    mockRoutes.splice(routeIndex, 1)
    return new HttpResponse(null, { status: 204 })
  }),

  http.put('/prock/api/mock-routes/:id/enable', ({ params }) => {
    const id = parseInt(params.id as string)
    const route = mockRoutes.find(r => r.id === id)
    
    if (!route) {
      return HttpResponse.json({ message: 'Route not found' }, { status: 404 })
    }
    
    route.enabled = true
    return HttpResponse.json(route)
  }),

  http.put('/prock/api/mock-routes/:id/disable', ({ params }) => {
    const id = parseInt(params.id as string)
    const route = mockRoutes.find(r => r.id === id)
    
    if (!route) {
      return HttpResponse.json({ message: 'Route not found' }, { status: 404 })
    }
    
    route.enabled = false
    return HttpResponse.json(route)
  }),

  // OpenAPI Documents API
  http.get('/prock/api/openapi/documents', () => {
    return HttpResponse.json(mockOpenApiDocuments)
  }),

  http.get('/prock/api/openapi/documents/:id', ({ params }) => {
    const id = params.id as string
    const document = mockOpenApiDocuments.find(d => d.documentId === id)
    
    if (!document) {
      return HttpResponse.json({ message: 'Document not found' }, { status: 404 })
    }
    
    return HttpResponse.json(document)
  }),

  http.post('/prock/api/openapi/documents', async ({ request }) => {
    const newDocument = await request.json() as any
    const createdDocument = {
      documentId: (mockOpenApiDocuments.length + 1).toString(),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      isActive: true,
      pathCount: 0,
      openApiVersion: '3.0.1',
      ...newDocument
    }
    mockOpenApiDocuments.push(createdDocument)
    return HttpResponse.json(createdDocument, { status: 201 })
  }),

  http.put('/prock/api/openapi/documents/:id', async ({ params, request }) => {
    const id = params.id as string
    const updatedDocument = await request.json() as any
    const documentIndex = mockOpenApiDocuments.findIndex(d => d.documentId === id)
    
    if (documentIndex === -1) {
      return HttpResponse.json({ message: 'Document not found' }, { status: 404 })
    }
    
    mockOpenApiDocuments[documentIndex] = {
      ...mockOpenApiDocuments[documentIndex],
      ...updatedDocument,
      updatedAt: new Date().toISOString()
    }
    return HttpResponse.json(mockOpenApiDocuments[documentIndex])
  }),

  http.delete('/prock/api/openapi/documents/:id', ({ params }) => {
    const id = params.id as string
    const documentIndex = mockOpenApiDocuments.findIndex(d => d.documentId === id)
    
    if (documentIndex === -1) {
      return HttpResponse.json({ message: 'Document not found' }, { status: 404 })
    }
    
    mockOpenApiDocuments.splice(documentIndex, 1)
    return new HttpResponse(null, { status: 204 })
  }),

  http.get('/prock/api/openapi/documents/:id/json', ({ params }) => {
    const id = params.id as string
    const document = mockOpenApiDocuments.find(d => d.documentId === id)
    
    if (!document) {
      return HttpResponse.json({ message: 'Document not found' }, { status: 404 })
    }
    
    // Return a mock OpenAPI document
    const mockOpenApiJson = {
      openapi: '3.0.1',
      info: {
        title: document.title,
        version: document.version,
        description: document.description
      },
      paths: {
        '/api/test': {
          get: {
            summary: 'Test endpoint',
            responses: {
              '200': {
                description: 'Success'
              }
            }
          }
        }
      }
    }
    
    return HttpResponse.json(mockOpenApiJson)
  }),

  http.post('/prock/api/openapi/documents/:id/generate-mocks', ({ params }) => {
    const id = params.id as string
    const document = mockOpenApiDocuments.find(d => d.documentId === id)
    
    if (!document) {
      return HttpResponse.json({ message: 'Document not found' }, { status: 404 })
    }
    
    // Mock generated routes
    const generatedRoutes = [
      {
        id: mockRoutes.length + 1,
        routeId: `generated-route-1`,
        method: 'GET',
        path: '/api/generated/test',
        httpStatusCode: 200,
        enabled: true,
        mock: { generated: true }
      }
    ]
    
    return HttpResponse.json(generatedRoutes, { status: 201 })
  }),

  // Config API
  http.get('/prock/api/config', () => {
    return HttpResponse.json(mockConfig)
  }),

  http.put('/prock/api/config/upstream-url', async ({ request }) => {
    const { upstreamUrl } = await request.json() as any
    mockConfig.upstreamUrl = upstreamUrl
    return HttpResponse.json(mockConfig)
  }),

  // HTTP Status Codes API
  http.get('/prock/api/http-status-codes', () => {
    return HttpResponse.json(mockStatusCodes)
  }),

  // HTTP Content Types API
  http.get('/prock/api/http-content-types', () => {
    return HttpResponse.json(mockContentTypes)
  }),

  // Restart API
  http.post('/prock/api/restart', () => {
    return HttpResponse.json({ message: 'Server restarted' })
  })
]
