import { describe, it, expect } from 'vitest'
import * as api from '../network/api'

describe('API Layer', () => {
  describe('Mock Routes API', () => {
    it('should fetch all mock routes', async () => {
      const response = await api.fetchRoutesAsync()
      
      expect(response.status).toBe(200)
      expect(response.data).toHaveLength(2)
      expect(response.data[0]).toMatchObject({
        id: 1,
        routeId: 'route-1',
        method: 'GET',
        path: '/api/test',
        enabled: true
      })
    })

    it('should create a new mock route', async () => {
      const newRoute = {
        method: 'POST',
        path: '/api/new-test',
        httpStatusCode: 201,
        enabled: true,
        mock: { success: true }
      }

      const response = await api.createNewRouteAsync(newRoute)
      
      expect(response.status).toBe(201)
      expect(response.data).toMatchObject({
        ...newRoute,
        id: expect.any(Number),
        routeId: expect.any(String)
      })
    })

    it('should update an existing mock route', async () => {
      const routeId = 1
      const updatedRoute = {
        method: 'PUT',
        path: '/api/updated-test',
        httpStatusCode: 200,
        enabled: false,
        mock: { updated: true }
      }

      const response = await api.updateRouteAsync(routeId, updatedRoute)
      
      expect(response.status).toBe(200)
      expect(response.data).toMatchObject({
        id: routeId,
        ...updatedRoute
      })
    })

    it('should delete a mock route', async () => {
      const routeId = 1

      const response = await api.deleteRouteAsync(routeId)
      
      expect(response.status).toBe(204)
    })

    it('should enable a mock route', async () => {
      const routeId = 2

      const response = await api.enableRouteAsync(routeId)
      
      expect(response.status).toBe(200)
      expect(response.data.enabled).toBe(true)
    })

    it('should disable a mock route', async () => {
      const routeId = 1

      const response = await api.disableRouteAsync(routeId)
      
      expect(response.status).toBe(200)
      expect(response.data.enabled).toBe(false)
    })

    it('should handle 404 error when updating non-existent route', async () => {
      const routeId = 999
      const updatedRoute = { method: 'GET', path: '/test' }

      await expect(api.updateRouteAsync(routeId, updatedRoute))
        .rejects.toThrow()
    })
  })

  describe('OpenAPI Documents API', () => {
    it('should fetch all OpenAPI documents', async () => {
      const response = await api.fetchOpenApiDocumentsAsync()
      
      expect(response.status).toBe(200)
      expect(response.data).toHaveLength(2)
      expect(response.data[0]).toMatchObject({
        documentId: '1',
        title: 'Test API',
        version: '1.0.0',
        isActive: true
      })
    })

    it('should fetch OpenAPI document by ID', async () => {
      const documentId = 1

      const response = await api.fetchOpenApiDocumentByIdAsync(documentId)
      
      expect(response.status).toBe(200)
      expect(response.data).toMatchObject({
        documentId: '1',
        title: 'Test API'
      })
    })

    it('should create a new OpenAPI document', async () => {
      const newDocument = {
        title: 'New API',
        version: '1.0.0',
        description: 'A new API',
        originalJson: '{"openapi": "3.0.1", "info": {"title": "New API"}}'
      }

      const response = await api.createOpenApiDocumentAsync(newDocument)
      
      expect(response.status).toBe(201)
      expect(response.data).toMatchObject({
        documentId: expect.any(String),
        title: 'New API',
        version: '1.0.0',
        isActive: true
      })
    })

    it('should update an OpenAPI document', async () => {
      const documentId = 1
      const updatedDocument = {
        title: 'Updated API',
        description: 'Updated description',
        isActive: false
      }

      const response = await api.updateOpenApiDocumentAsync(documentId, updatedDocument)
      
      expect(response.status).toBe(200)
      expect(response.data).toMatchObject({
        documentId: '1',
        title: 'Updated API',
        isActive: false
      })
    })

    it('should delete an OpenAPI document', async () => {
      const documentId = 1

      const response = await api.deleteOpenApiDocumentAsync(documentId)
      
      expect(response.status).toBe(204)
    })

    it('should generate mock routes from OpenAPI document', async () => {
      const documentId = 1

      const response = await api.generateMockRoutesFromOpenApi(documentId)
      
      expect(response.status).toBe(201)
      expect(response.data).toHaveLength(1)
      expect(response.data[0]).toMatchObject({
        id: expect.any(Number),
        routeId: expect.any(String),
        method: 'GET',
        path: '/api/generated/test'
      })
    })

    it('should handle 404 error when fetching non-existent document', async () => {
      const documentId = 999

      await expect(api.fetchOpenApiDocumentByIdAsync(documentId))
        .rejects.toThrow()
    })
  })

  describe('Configuration API', () => {
    it('should fetch server configuration', async () => {
      const response = await api.fetchServerConfigAsync()
      
      expect(response.status).toBe(200)
      expect(response.data).toMatchObject({
        connectionString: 'test-connection',
        upstreamUrl: 'http://localhost:3000',
        host: 'localhost',
        port: '5001'
      })
    })

    it('should update upstream URL', async () => {
      const newUpstreamUrl = 'http://localhost:4000'

      const response = await api.updateUpstreamUrlAsync(newUpstreamUrl)
      
      expect(response.status).toBe(200)
      expect(response.data.upstreamUrl).toBe(newUpstreamUrl)
    })
  })

  describe('HTTP Codes and Content Types API', () => {
    it('should fetch HTTP status codes', async () => {
      const response = await api.fetchHttpStatusCodesAsync()
      
      expect(response.status).toBe(200)
      expect(response.data).toHaveLength(5)
      expect(response.data[0]).toHaveProperty('200')
    })

    it('should fetch HTTP content types', async () => {
      const response = await api.fetchHttpContentTypesAsync()
      
      expect(response.status).toBe(200)
      expect(response.data).toHaveLength(4)
      expect(response.data).toContain('application/json')
    })
  })

  describe('System API', () => {
    it('should restart the server', async () => {
      const response = await api.restartAsync()
      
      expect(response.status).toBe(200)
      expect(response.data).toMatchObject({
        message: 'Server restarted'
      })
    })
  })
})


