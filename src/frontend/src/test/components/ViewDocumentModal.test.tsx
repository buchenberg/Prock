import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '../utils'
import { userEvent } from '@testing-library/user-event'
import ViewDocumentModal from '../../components/OpenApiDocuments/ViewDocumentModal'
import { OpenApiDocument } from '../../store/useOpenApiStore'

// Mock the stores and navigation
const mockGenerateMockRoutesFromOpenApi = vi.fn()
const mockNavigate = vi.fn()

vi.mock('../../store/useProckStore', () => ({
  useProckStore: () => ({
    generateMockRoutesFromOpenApi: mockGenerateMockRoutesFromOpenApi
  })
}))

vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate
}))

// Sample OpenAPI document for testing
const mockDocument: OpenApiDocument = {
  documentId: '1',
  title: 'Test API',
  version: '1.0.0',
  description: 'Test API Description',
  openApiVersion: '3.0.0',
  createdAt: '2023-01-01T00:00:00Z',
  updatedAt: '2023-01-01T00:00:00Z',
  isActive: true,
  pathCount: 2
}

describe('ViewDocumentModal', () => {
  const defaultProps = {
    showDetailModal: true,
    setShowDetailModal: vi.fn(),
    selectedDocument: mockDocument,
    setShowJsonModal: vi.fn(),
    onNavigateToMocks: vi.fn()
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render the modal with document details', () => {
    render(<ViewDocumentModal {...defaultProps} />)
    
    expect(screen.getByText('OpenAPI Document Details')).toBeInTheDocument()
    expect(screen.getByText('Test API')).toBeInTheDocument()
    expect(screen.getByText('Test API Description')).toBeInTheDocument()
    expect(screen.getByText('1.0.0')).toBeInTheDocument()
    expect(screen.getByText('3.0.0')).toBeInTheDocument()
  })

  it('should show generate mock routes button', () => {
    render(<ViewDocumentModal {...defaultProps} />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    expect(generateButton).toBeInTheDocument()
    expect(generateButton).not.toBeDisabled()
  })

  it('should call generateMockRoutesFromOpenApi when generate button is clicked', async () => {
    const user = userEvent.setup()
    mockGenerateMockRoutesFromOpenApi.mockResolvedValue({ data: [] })
    
    render(<ViewDocumentModal {...defaultProps} />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    await user.click(generateButton)
    
    expect(mockGenerateMockRoutesFromOpenApi).toHaveBeenCalledWith(1)
  })

  it('should close modal and navigate to mocks tab on successful generation', async () => {
    const user = userEvent.setup()
    const setShowDetailModal = vi.fn()
    const onNavigateToMocks = vi.fn()
    
    // Mock successful generation
    mockGenerateMockRoutesFromOpenApi.mockResolvedValue({ data: [{ id: 1, path: '/test' }] })
    
    render(<ViewDocumentModal 
      {...defaultProps} 
      setShowDetailModal={setShowDetailModal}
      onNavigateToMocks={onNavigateToMocks}
    />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    await user.click(generateButton)
    
    await waitFor(() => {
      expect(setShowDetailModal).toHaveBeenCalledWith(false)
      expect(onNavigateToMocks).toHaveBeenCalled()
    })
  })

  it('should show loading state while generating mocks', async () => {
    const user = userEvent.setup()
    
    // Mock a promise that doesn't resolve immediately
    let resolvePromise: (value: any) => void
    const pendingPromise = new Promise((resolve) => {
      resolvePromise = resolve
    })
    mockGenerateMockRoutesFromOpenApi.mockReturnValue(pendingPromise)
    
    render(<ViewDocumentModal {...defaultProps} />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    await user.click(generateButton)
    
    // Check loading state
    expect(screen.getByText(/generating/i)).toBeInTheDocument()
    expect(generateButton).toBeDisabled()
    expect(document.querySelector('.spinner-border')).toBeInTheDocument() // Spinner
    
    // Resolve the promise to complete the test
    resolvePromise!({ data: [] })
  })

  it('should show error message on generation failure and not close modal', async () => {
    const user = userEvent.setup()
    const setShowDetailModal = vi.fn()
    const onNavigateToMocks = vi.fn()
    
    // Mock failed generation with 409 Conflict
    const errorMessage = 'All 2 mock routes already exist: GET /test/new, GET /users'
    mockGenerateMockRoutesFromOpenApi.mockRejectedValue(new Error(errorMessage))
    
    render(<ViewDocumentModal 
      {...defaultProps} 
      setShowDetailModal={setShowDetailModal}
      onNavigateToMocks={onNavigateToMocks}
    />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    await user.click(generateButton)
    
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument()
      expect(screen.getByText(errorMessage)).toBeInTheDocument()
    })
    
    // Modal should NOT close and navigation should NOT happen
    expect(setShowDetailModal).not.toHaveBeenCalledWith(false)
    expect(onNavigateToMocks).not.toHaveBeenCalled()
  })

  it('should allow dismissing error alert', async () => {
    const user = userEvent.setup()
    
    // Mock failed generation
    mockGenerateMockRoutesFromOpenApi.mockRejectedValue(new Error('Some error'))
    
    render(<ViewDocumentModal {...defaultProps} />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    await user.click(generateButton)
    
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument()
    })
    
    // Dismiss the alert (specifically the alert close button)
    const closeButton = screen.getByRole('button', { name: /close alert/i })
    await user.click(closeButton)
    
    await waitFor(() => {
      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    })
  })

  it('should clear error when modal is closed', async () => {
    const user = userEvent.setup()
    const setShowDetailModal = vi.fn()
    
    // Mock failed generation to show error
    mockGenerateMockRoutesFromOpenApi.mockRejectedValue(new Error('Some error'))
    
    render(<ViewDocumentModal 
      {...defaultProps} 
      setShowDetailModal={setShowDetailModal}
    />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    await user.click(generateButton)
    
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument()
    })
    
    // Close the modal via the close button
    const modalCloseButton = screen.getByLabelText('Close')
    await user.click(modalCloseButton)
    
    expect(setShowDetailModal).toHaveBeenCalledWith(false)
  })

  it('should clear error when starting a new generation attempt', async () => {
    const user = userEvent.setup()
    
    // First, cause an error
    mockGenerateMockRoutesFromOpenApi.mockRejectedValueOnce(new Error('First error'))
    
    render(<ViewDocumentModal {...defaultProps} />)
    
    const generateButton = screen.getByRole('button', { name: /generate mock routes/i })
    await user.click(generateButton)
    
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument()
    })
    
    // Then mock a successful attempt
    mockGenerateMockRoutesFromOpenApi.mockResolvedValue({ data: [] })
    
    // Click generate again
    await user.click(generateButton)
    
    // Error should be cleared immediately when starting new attempt
    await waitFor(() => {
      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    })
  })
})
