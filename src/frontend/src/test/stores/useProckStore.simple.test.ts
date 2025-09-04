import { describe, it, expect, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useProckStore } from '../../store/useProckStore'

describe('useProckStore - Basic Functionality', () => {
  beforeEach(() => {
    // Reset store state before each test
    const initialState = useProckStore.getState()
    useProckStore.setState({
      ...initialState,
      httpContentTypes: [],
      httpStatusCodes: [],
      mockRoutes: { isLoading: false, isError: false },
      prockConfig: { isLoading: false, isError: false }
    })
  })

  it('should have initial state', () => {
    const { result } = renderHook(() => useProckStore())

    expect(result.current.httpContentTypes).toEqual([])
    expect(result.current.httpStatusCodes).toEqual([])
    expect(result.current.mockRoutes.isLoading).toBe(false)
    expect(result.current.mockRoutes.isError).toBe(false)
    expect(result.current.prockConfig.isLoading).toBe(false)
    expect(result.current.prockConfig.isError).toBe(false)
  })

  it('should fetch HTTP content types', async () => {
    const { result } = renderHook(() => useProckStore())

    await act(async () => {
      await result.current.getHttpContentTypes()
    })

    expect(result.current.httpContentTypes).toHaveLength(4)
    expect(result.current.httpContentTypes).toContain('application/json')
  })

  it('should fetch HTTP status codes', async () => {
    const { result } = renderHook(() => useProckStore())

    await act(async () => {
      await result.current.getHttpStatusCodes()
    })

    expect(result.current.httpStatusCodes).toHaveLength(5)
    expect(result.current.httpStatusCodes[0]).toHaveProperty('200')
  })

  it('should fetch mock routes', async () => {
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

  it('should fetch prock configuration', async () => {
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

  it('should create a new mock route', async () => {
    const { result } = renderHook(() => useProckStore())

    // First load existing routes
    await act(async () => {
      await result.current.getMockRoutes()
    })

    expect(result.current.mockRoutes.value).toHaveLength(2)

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
    
    // The store should contain the new route
    const createdRoute = result.current.mockRoutes.value?.find(r => r.path === '/api/new-test')
    expect(createdRoute).toMatchObject({
      method: 'POST',
      path: '/api/new-test',
      httpStatusCode: 201
    })
  })

  it('should handle update mock route with ID', async () => {
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
})


