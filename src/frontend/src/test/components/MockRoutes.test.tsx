import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '../utils'
import { userEvent } from '@testing-library/user-event'
import MockRoutes from '../../components/MockRoutes'

// Mock the store
const mockGetMockRoutes = vi.fn()
const mockCreateMockRoute = vi.fn()
const mockUpdateMockRoute = vi.fn()
const mockDeleteMockRoute = vi.fn()

vi.mock('../../store/useProckStore', () => ({
  useProckStore: (selector: any) => {
    const mockState = {
      mockRoutes: {
        isLoading: false,
        isError: false,
        value: [
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
      },
      getMockRoutes: mockGetMockRoutes,
      createMockRoute: mockCreateMockRoute,
      updateMockRoute: mockUpdateMockRoute,
      deleteMockRoute: mockDeleteMockRoute
    }
    
    return selector(mockState)
  }
}))

// Mock API functions
vi.mock('../../network/api', () => ({
  enableRouteAsync: vi.fn(),
  disableRouteAsync: vi.fn()
}))

describe('MockRoutes Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render mock routes list', () => {
    render(<MockRoutes />)
    
    expect(screen.getByText('GET')).toBeInTheDocument()
    expect(screen.getByText('/api/test')).toBeInTheDocument()
    expect(screen.getByText('POST')).toBeInTheDocument()
    expect(screen.getByText('/api/users')).toBeInTheDocument()
  })

  it('should show loading state', () => {
    // Mock loading state
    vi.mocked(require('../../store/useProckStore').useProckStore).mockImplementation((selector: any) => {
      const mockState = {
        mockRoutes: {
          isLoading: true,
          isError: false,
          value: undefined
        },
        getMockRoutes: mockGetMockRoutes,
        createMockRoute: mockCreateMockRoute,
        updateMockRoute: mockUpdateMockRoute,
        deleteMockRoute: mockDeleteMockRoute
      }
      return selector(mockState)
    })

    render(<MockRoutes />)
    
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('should show error state', () => {
    // Mock error state
    vi.mocked(require('../../store/useProckStore').useProckStore).mockImplementation((selector: any) => {
      const mockState = {
        mockRoutes: {
          isLoading: false,
          isError: true,
          errorMessage: 'Failed to load routes'
        },
        getMockRoutes: mockGetMockRoutes,
        createMockRoute: mockCreateMockRoute,
        updateMockRoute: mockUpdateMockRoute,
        deleteMockRoute: mockDeleteMockRoute
      }
      return selector(mockState)
    })

    render(<MockRoutes />)
    
    expect(screen.getByText(/failed to load routes/i)).toBeInTheDocument()
  })

  it('should display correct method badges', () => {
    render(<MockRoutes />)
    
    // Check for GET badge
    const getBadge = screen.getByText('GET')
    expect(getBadge).toBeInTheDocument()
    expect(getBadge.closest('.badge')).toHaveClass('bg-success')
    
    // Check for POST badge
    const postBadge = screen.getByText('POST')
    expect(postBadge).toBeInTheDocument()
    expect(postBadge.closest('.badge')).toHaveClass('bg-primary')
  })

  it('should show enabled/disabled switches correctly', () => {
    render(<MockRoutes />)
    
    const switches = screen.getAllByRole('switch')
    expect(switches).toHaveLength(2)
    
    // First route should be enabled
    expect(switches[0]).toBeChecked()
    
    // Second route should be disabled
    expect(switches[1]).not.toBeChecked()
  })

  it('should display mock response content', () => {
    render(<MockRoutes />)
    
    expect(screen.getByText(/"message": "Hello World"/)).toBeInTheDocument()
    expect(screen.getByText(/"name": "John Doe"/)).toBeInTheDocument()
  })

  it('should open create modal when Add Route button is clicked', async () => {
    const user = userEvent.setup()
    render(<MockRoutes />)
    
    // Look for the plus icon instead of text
    const addButton = screen.getByTitle(/add route/i) || screen.getByRole('button', { name: /add/i }) || screen.getByTestId('add-route-button')
    if (!addButton) {
      // Fallback: look for the SVG icon
      const plusIcon = screen.getByText('Mock Routes').parentElement?.querySelector('svg')
      expect(plusIcon).toBeInTheDocument()
      if (plusIcon) {
        await user.click(plusIcon)
      }
    } else {
      await user.click(addButton)
    }
    
    expect(screen.getByText(/create mock route/i)).toBeInTheDocument()
  })

  it('should open edit modal when edit button is clicked', async () => {
    const user = userEvent.setup()
    render(<MockRoutes />)
    
    const editButtons = screen.getAllByLabelText(/edit/i)
    await user.click(editButtons[0])
    
    expect(screen.getByText(/edit mock route/i)).toBeInTheDocument()
  })

  it('should open delete modal when delete button is clicked', async () => {
    const user = userEvent.setup()
    render(<MockRoutes />)
    
    const deleteButtons = screen.getAllByLabelText(/delete/i)
    await user.click(deleteButtons[0])
    
    expect(screen.getByText(/delete mock route/i)).toBeInTheDocument()
    expect(screen.getByText(/are you sure you want to delete this mock route/i)).toBeInTheDocument()
  })

  it('should call deleteMockRoute when delete is confirmed', async () => {
    const user = userEvent.setup()
    render(<MockRoutes />)
    
    // Open delete modal
    const deleteButtons = screen.getAllByLabelText(/delete/i)
    await user.click(deleteButtons[0])
    
    // Confirm deletion
    const confirmButton = screen.getByRole('button', { name: /delete/i })
    await user.click(confirmButton)
    
    expect(mockDeleteMockRoute).toHaveBeenCalledWith(1)
  })

  it('should handle route enable/disable toggle', async () => {
    const user = userEvent.setup()
    render(<MockRoutes />)
    
    const switches = screen.getAllByRole('switch')
    
    // Click the first switch (currently enabled, should disable)
    await user.click(switches[0])
    
    // The API should be called (mocked)
    await waitFor(() => {
      expect(require('../../network/api').disableRouteAsync).toHaveBeenCalledWith(1)
    })
  })

  it('should show empty state when no routes exist', () => {
    // Mock empty state
    vi.mocked(require('../../store/useProckStore').useProckStore).mockImplementation((selector: any) => {
      const mockState = {
        mockRoutes: {
          isLoading: false,
          isError: false,
          value: []
        },
        getMockRoutes: mockGetMockRoutes,
        createMockRoute: mockCreateMockRoute,
        updateMockRoute: mockUpdateMockRoute,
        deleteMockRoute: mockDeleteMockRoute
      }
      return selector(mockState)
    })

    render(<MockRoutes />)
    
    expect(screen.getByText(/no mock routes available/i)).toBeInTheDocument()
  })

  it('should call getMockRoutes on component mount', () => {
    render(<MockRoutes />)
    
    expect(mockGetMockRoutes).toHaveBeenCalled()
  })
})
