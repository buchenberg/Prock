# Frontend Test Suite

This directory contains comprehensive unit and integration tests for the Prock frontend application.

## Test Structure

### 📁 `/test/`
- `setup.ts` - Test configuration and global setup
- `utils.tsx` - Custom testing utilities and helpers

### 📁 `/test/mocks/`
- `server.ts` - MSW (Mock Service Worker) server setup
- `handlers.ts` - API request handlers for mocking backend responses

### 📁 `/test/stores/`
- `useProckStore.test.ts` - Comprehensive Zustand store tests for mock routes and configuration
- `useProckStore.simple.test.ts` - Basic functionality tests for Zustand store
- `useOpenApiStore.test.ts` - OpenAPI document store tests

### 📁 `/test/components/`
- `MockRoutes.test.tsx` - React component tests for MockRoutes component

### 📁 `/test/integration/`
- `mockRoutes.integration.test.tsx` - End-to-end integration tests

### 📁 `/test/`
- `api.test.ts` - API layer unit tests for all endpoints

## Test Coverage

### ✅ API Layer Tests (19 tests)
- **Mock Routes API**: CRUD operations, enable/disable functionality
- **OpenAPI Documents API**: Document management, mock route generation
- **Configuration API**: Server config and upstream URL management
- **HTTP Utilities**: Status codes and content types
- **System API**: Server restart functionality

### ✅ Store Tests (7+ tests)
- **State Management**: Initial state, loading states, error handling
- **Mock Routes**: Fetch, create, update, delete operations
- **Configuration**: Config fetching and URL updates
- **OpenAPI Documents**: Document lifecycle management

### ✅ Component Tests (8+ tests)
- **Rendering**: Proper display of routes, badges, switches
- **User Interaction**: Modal opening, form submission, route toggling
- **State Handling**: Loading, error, and empty states
- **Accessibility**: Button labels, form controls

### ✅ Integration Tests (6+ tests)
- **API-Store Integration**: Data flow from API through store to UI
- **User Workflows**: Complete user interactions end-to-end
- **Error Handling**: Graceful degradation and error states

## Key Testing Features

### 🎯 **Modern Testing Stack**
- **Vitest**: Fast, modern test runner
- **React Testing Library**: Component testing with best practices
- **MSW**: API mocking without network requests
- **User Events**: Realistic user interaction simulation

### 🔄 **Test Isolation**
- Each test runs in isolation with fresh mock data
- Store state reset between tests
- No test dependencies or side effects

### 📊 **API Contract Testing**
- Tests verify new integer ID format (not string GUIDs)
- Validates updated endpoint URLs (`/prock/api/openapi/documents`)
- Confirms new property names (`originalJson` vs `openApiJson`)

### 🎨 **UI Testing**
- Component rendering validation
- User interaction testing
- Accessibility compliance checks
- Error boundary testing

## Running Tests

```bash
# Run all tests
npm run test

# Run tests once
npm run test:run

# Run tests with coverage
npm run test:coverage

# Run specific test file
npm run test:run -- src/test/api.test.ts

# Run tests in watch mode
npm run test
```

## Test Data

The test suite uses realistic mock data that matches the production API:

### Mock Routes
- GET `/api/test` (enabled)
- POST `/api/users` (disabled)

### OpenAPI Documents
- "Test API" v1.0.0 (active)
- "User API" v2.0.0 (inactive)

### Configuration
- Connection string, upstream URL, host, port

## Best Practices

1. **Test Behavior, Not Implementation**: Tests focus on user-visible behavior
2. **Isolated Tests**: Each test is independent and can run in any order
3. **Realistic Mocking**: MSW provides realistic API responses
4. **Accessibility First**: Tests include accessibility checks
5. **Error Scenarios**: Tests cover both success and failure cases

## Integration with Backend

These tests validate the frontend's compatibility with the new MariaDB backend:

- ✅ Integer IDs instead of string GUIDs
- ✅ Updated API endpoints
- ✅ New property names in requests/responses
- ✅ Error handling for updated error codes
- ✅ State management with new data structures
