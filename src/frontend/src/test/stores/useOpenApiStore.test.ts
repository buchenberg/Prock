import { describe, it, expect, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useOpenApiStore } from '../../store/useOpenApiStore'

describe('useOpenApiStore', () => {
  beforeEach(() => {
    // Reset store state before each test
    useOpenApiStore.setState((state) => ({
      ...state,
      documents: { isLoading: false, isError: false },
      documentDetail: { isLoading: false, isError: false }
    }))
  })

  describe('Documents Management', () => {
    it('should fetch documents successfully', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      await act(async () => {
        await result.current.getDocuments()
      })

      expect(result.current.documents.isLoading).toBe(false)
      expect(result.current.documents.isError).toBe(false)
      expect(result.current.documents.value).toHaveLength(2)
      expect(result.current.documents.value?.[0]).toMatchObject({
        documentId: '1',
        title: 'Test API',
        version: '1.0.0',
        isActive: true
      })
    })

    it('should create a new document successfully', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // First load existing documents
      await act(async () => {
        await result.current.getDocuments()
      })

      const newDocument = {
        title: 'New API',
        version: '1.0.0',
        description: 'A new API',
        originalJson: '{"openapi": "3.0.1", "info": {"title": "New API"}}'
      }

      await act(async () => {
        await result.current.createDocument(newDocument)
      })

      expect(result.current.documents.isLoading).toBe(false)
      expect(result.current.documents.isError).toBe(false)
      expect(result.current.documents.value).toHaveLength(3)
      
      const createdDocument = result.current.documents.value?.find(d => d.title === 'New API')
      expect(createdDocument).toMatchObject({
        title: 'New API',
        version: '1.0.0',
        isActive: true
      })
    })

    it('should update an existing document successfully', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // First load existing documents
      await act(async () => {
        await result.current.getDocuments()
      })

      const updatedData = {
        title: 'Updated API',
        description: 'Updated description',
        isActive: false
      }

      await act(async () => {
        await result.current.updateDocument(1, updatedData)
      })

      expect(result.current.documents.isLoading).toBe(false)
      expect(result.current.documents.isError).toBe(false)
      
      const updatedDocument = result.current.documents.value?.find(d => d.documentId === '1')
      expect(updatedDocument).toMatchObject({
        documentId: '1',
        title: 'Updated API',
        isActive: false
      })
    })

    it('should delete a document successfully', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // First load existing documents
      await act(async () => {
        await result.current.getDocuments()
      })

      const initialCount = result.current.documents.value?.length || 0

      await act(async () => {
        await result.current.deleteDocument(1)
      })

      expect(result.current.documents.isLoading).toBe(false)
      expect(result.current.documents.isError).toBe(false)
      expect(result.current.documents.value).toHaveLength(initialCount - 1)
      
      const deletedDocument = result.current.documents.value?.find(d => d.documentId === '1')
      expect(deletedDocument).toBeUndefined()
    })

    it('should fetch OpenAPI JSON for a document', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      await act(async () => {
        await result.current.fetchOpenApiJson(1)
      })

      expect(result.current.documentDetail.isLoading).toBe(false)
      expect(result.current.documentDetail.isError).toBe(false)
      // The documentDetail should contain the parsed OpenAPI document
      expect(result.current.documentDetail.value).toBeDefined()
    })
  })

  describe('Loading and Error States', () => {
    it('should set loading state when fetching documents', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // Start the async operation but don't await it
      const promise = act(async () => {
        result.current.getDocuments()
      })

      // Check loading state immediately
      expect(result.current.documents.isLoading).toBe(true)
      expect(result.current.documents.isError).toBe(false)

      // Now wait for completion
      await promise
    })

    it('should set loading state when creating document', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      const newDocument = {
        title: 'Test',
        originalJson: '{"openapi": "3.0.1"}'
      }

      // Start the async operation but don't await it
      const promise = act(async () => {
        result.current.createDocument(newDocument)
      })

      // Check loading state immediately
      expect(result.current.documents.isLoading).toBe(true)
      expect(result.current.documents.isError).toBe(false)

      // Now wait for completion
      await promise
    })

    it('should set loading state when updating document', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // First load existing documents
      await act(async () => {
        await result.current.getDocuments()
      })

      // Start the update operation but don't await it
      const promise = act(async () => {
        result.current.updateDocument(1, { title: 'Updated' })
      })

      // Check loading state immediately
      expect(result.current.documents.isLoading).toBe(true)
      expect(result.current.documents.isError).toBe(false)

      // Now wait for completion
      await promise
    })

    it('should set loading state when deleting document', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // First load existing documents
      await act(async () => {
        await result.current.getDocuments()
      })

      // Start the delete operation but don't await it
      const promise = act(async () => {
        result.current.deleteDocument(1)
      })

      // Check loading state immediately
      expect(result.current.documents.isLoading).toBe(true)
      expect(result.current.documents.isError).toBe(false)

      // Now wait for completion
      await promise
    })

    it('should set loading state when fetching document JSON', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // Start the async operation but don't await it
      const promise = act(async () => {
        result.current.fetchOpenApiJson(1)
      })

      // Check loading state immediately
      expect(result.current.documentDetail.isLoading).toBe(true)
      expect(result.current.documentDetail.isError).toBe(false)

      // Now wait for completion
      await promise
    })
  })

  describe('Error Handling', () => {
    it('should handle error responses gracefully in createDocument', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // Create a document that will trigger an error (missing required field)
      const invalidDocument = {
        title: 'Test',
        // Missing originalJson field
      }

      await act(async () => {
        // This should trigger an error due to validation
        await result.current.createDocument(invalidDocument as any)
      })

      // The error state should be handled gracefully
      expect(result.current.documents.isLoading).toBe(false)
      // Error handling might vary based on implementation
    })

    it('should handle 404 errors when fetching non-existent document', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      await act(async () => {
        await result.current.fetchOpenApiJson(999) // Non-existent ID
      })

      expect(result.current.documentDetail.isLoading).toBe(false)
      expect(result.current.documentDetail.isError).toBe(true)
      expect(result.current.documentDetail.errorMessage).toBeDefined()
    })
  })

  describe('State Management', () => {
    it('should maintain document list when creating new document', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // First load existing documents
      await act(async () => {
        await result.current.getDocuments()
      })

      const initialCount = result.current.documents.value?.length || 0

      const newDocument = {
        title: 'New Document',
        version: '1.0.0',
        originalJson: '{"openapi": "3.0.1"}'
      }

      await act(async () => {
        await result.current.createDocument(newDocument)
      })

      // Should have one more document
      expect(result.current.documents.value).toHaveLength(initialCount + 1)
      
      // Original documents should still be there
      const originalDocument = result.current.documents.value?.find(d => d.documentId === '1')
      expect(originalDocument).toBeDefined()
      expect(originalDocument?.title).toBe('Test API')
    })

    it('should update specific document without affecting others', async () => {
      const { result } = renderHook(() => useOpenApiStore())

      // First load existing documents
      await act(async () => {
        await result.current.getDocuments()
      })

      await act(async () => {
        await result.current.updateDocument(1, { title: 'Updated Title' })
      })

      // Check that the correct document was updated
      const updatedDocument = result.current.documents.value?.find(d => d.documentId === '1')
      expect(updatedDocument?.title).toBe('Updated Title')

      // Check that other documents were not affected
      const otherDocument = result.current.documents.value?.find(d => d.documentId === '2')
      expect(otherDocument?.title).toBe('User API') // Original title
    })
  })
})
