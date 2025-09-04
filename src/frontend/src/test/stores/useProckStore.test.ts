import { describe, it, expect, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useProckStore } from '../../store/useProckStore'

describe('useProckStore', () => {
  beforeEach(() => {
    // Reset store state before each test
    useProckStore.setState((state) => ({
      ...state,
      httpContentTypes: [],
      httpStatusCodes: [],
      mockRoutes: { isLoading: false, isError: false },
      prockConfig: { isLoading: false, isError: false }
    }))
  })

  describe('HTTP Content Types', () => {
    it('should fetch HTTP content types successfully', async () => {
      const { result } = renderHook(() => useProckStore())

      await act(async () => {
        await result.current.getHttpContentTypes()
      })

      expect(result.current.httpContentTypes).toHaveLength(4)
      expect(result.current.httpContentTypes).toContain('application/json')
    })
  })

  describe('HTTP Status Codes', () => {
    it('should fetch HTTP status codes successfully', async () => {
      const { result } = renderHook(() => useProckStore())

      await act(async () => {
        await result.current.getHttpStatusCodes()
      })

      expect(result.current.httpStatusCodes).toHaveLength(5)
      expect(result.current.httpStatusCodes[0]).toHaveProperty('200')
    })
  })

  describe('Mock Routes', () => {
    it('should fetch mock routes successfully', async () => {
      const { result } = renderHook(() => useProckStore())

      await act(async () => {
        await result.current.getMockRoutes()
      })

      expect(result.current.mockRoutes.isLoading).toBe(false)
      expect(result.current.mockRoutes.isError).toBe(false)
      expect(result.current.mockRoutes.value).toHaveLength(2)
      expect(result.current.mockRoutes.value?.[0]).toMatchObject({
        id: 1,
        routeId: 'route-1',
        method: 'GET',
        path: '/api/test'
      })
    })

    it('should create a new mock route', async () => {
      const { result } = renderHook(() => useProckStore())

      // First load existing routes
      await act(async () => {
        await result.current.getMockRoutes()
      })

      const newRoute = {
        method: 'POST',
        path: '/api/new-test',
        httpStatusCode: 201,
        enabled: true,
        mock: { success: true }
      }

      await act(async () => {
        await result.current.createMockRoute(newRoute)
      })

      expect(result.current.mockRoutes.isLoading).toBe(false)
      expect(result.current.mockRoutes.isError).toBe(false)
      expect(result.current.mockRoutes.value).toHaveLength(3)
      
      const createdRoute = result.current.mockRoutes.value?.find(r => r.path === '/api/new-test')
      expect(createdRoute).toMatchObject({
        method: 'POST',
        path: '/api/new-test',
        httpStatusCode: 201
      })
    })

    it('should update an existing mock route', async () => {
      const { result } = renderHook(() => useProckStore())

      // First load existing routes
      await act(async () => {
        await result.current.getMockRoutes()
      })

      const routeToUpdate = {
        id: 1,
        method: 'PUT',
        path: '/api/updated-test',
        httpStatusCode: 200,
        enabled: false,
        mock: { updated: true }
      }

      await act(async () => {
        await result.current.updateMockRoute(routeToUpdate)
      })

      expect(result.current.mockRoutes.isLoading).toBe(false)
      expect(result.current.mockRoutes.isError).toBe(false)
      
      const updatedRoute = result.current.mockRoutes.value?.find(r => r.id === 1)
      expect(updatedRoute).toMatchObject({
        id: 1,
        method: 'PUT',
        path: '/api/updated-test',
        enabled: false
      })
    })

    it('should delete a mock route', async () => {
      const { result } = renderHook(() => useProckStore())

      // First load existing routes
      await act(async () => {
        await result.current.getMockRoutes()
      })

      const initialCount = result.current.mockRoutes.value?.length || 0

      await act(async () => {
        await result.current.deleteMockRoute(1)
      })

      expect(result.current.mockRoutes.isLoading).toBe(false)
      expect(result.current.mockRoutes.isError).toBe(false)
      expect(result.current.mockRoutes.value).toHaveLength(initialCount - 1)
      
      const deletedRoute = result.current.mockRoutes.value?.find(r => r.id === 1)
      expect(deletedRoute).toBeUndefined()
    })

    it('should handle error when updating route without ID', async () => {
      const { result } = renderHook(() => useProckStore())

      const invalidRoute = {
        method: 'POST',
        path: '/api/test',
        httpStatusCode: 200
      }

      await act(async () => {
        await result.current.updateMockRoute(invalidRoute)
      })

      expect(result.current.mockRoutes.isError).toBe(true)
      expect(result.current.mockRoutes.errorMessage).toContain('ID is required')
    })

    it('should generate mock routes from OpenAPI document', async () => {
      const { result } = renderHook(() => useProckStore())

      // First load existing routes
      await act(async () => {
        await result.current.getMockRoutes()
      })

      await act(async () => {
        await result.current.generateMockRoutesFromOpenApi(1)
      })

      expect(result.current.mockRoutes.isLoading).toBe(false)
      expect(result.current.mockRoutes.isError).toBe(false)
      expect(result.current.mockRoutes.value).toHaveLength(1)
      
      const generatedRoute = result.current.mockRoutes.value?.[0]
      expect(generatedRoute).toMatchObject({
        method: 'GET',
        path: '/api/generated/test',
        mock: { generated: true }
      })
    })
  })

  describe('Prock Configuration', () => {
    it('should fetch prock configuration successfully', async () => {
      const { result } = renderHook(() => useProckStore())

      await act(async () => {
        await result.current.getProckConfigs()
      })

      expect(result.current.prockConfig.isLoading).toBe(false)
      expect(result.current.prockConfig.isError).toBe(false)
      expect(result.current.prockConfig.value).toMatchObject({
        connectionString: 'test-connection',
        upstreamUrl: 'http://localhost:3000',
        host: 'localhost',
        port: '5001'
      })
    })

    it('should update upstream URL successfully', async () => {
      const { result } = renderHook(() => useProckStore())

      // First load existing config
      await act(async () => {
        await result.current.getProckConfigs()
      })

      const newUpstreamUrl = 'http://localhost:4000'

      await act(async () => {
        await result.current.updateUpstreamUrl(newUpstreamUrl)
      })

      expect(result.current.prockConfig.isLoading).toBe(false)
      expect(result.current.prockConfig.isError).toBe(false)
      expect(result.current.prockConfig.value?.upstreamUrl).toBe(newUpstreamUrl)
    })
  })

  describe('Loading and Error States', () => {
    it('should set loading state when fetching mock routes', async () => {
      const { result } = renderHook(() => useProckStore())

      // Start the async operation but don't await it
      const promise = act(async () => {
        result.current.getMockRoutes()
      })

      // Check loading state immediately
      expect(result.current.mockRoutes.isLoading).toBe(true)
      expect(result.current.mockRoutes.isError).toBe(false)

      // Now wait for completion
      await promise
    })

    it('should set loading state when fetching config', async () => {
      const { result } = renderHook(() => useProckStore())

      // Start the async operation but don't await it
      const promise = act(async () => {
        result.current.getProckConfigs()
      })

      // Check loading state immediately
      expect(result.current.prockConfig.isLoading).toBe(true)
      expect(result.current.prockConfig.isError).toBe(false)

      // Now wait for completion
      await promise
    })
  })
})
