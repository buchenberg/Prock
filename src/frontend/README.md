# Prock Frontend

Modern React + TypeScript frontend for the Prock mocking proxy application.

## Tech Stack

- **React 18** - Modern React with hooks and concurrent features
- **TypeScript** - Full type safety and developer experience
- **Vite** - Fast build tool with HMR and optimized builds
- **React Bootstrap** - Responsive UI components
- **Zustand** - Lightweight state management
- **React Router** - Client-side routing
- **Axios** - HTTP client for API communication
- **Monaco Editor** - Code editor for JSON editing
- **SignalR** - Real-time communication with backend

## Development

### Prerequisites

- Node.js 18+ 
- npm or yarn

### Setup

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

### Available Scripts

- `npm run dev` - Start development server with HMR
- `npm run build` - Build for production
- `npm run preview` - Preview production build locally
- `npm run lint` - Run ESLint
- `npm run test` - Run tests in watch mode
- `npm run test:run` - Run tests once
- `npm run test:coverage` - Run tests with coverage report

## Testing

Comprehensive test suite using modern testing tools:

### Test Stack

- **Vitest** - Fast test runner with Vite integration
- **React Testing Library** - Component testing utilities
- **MSW (Mock Service Worker)** - API mocking
- **User Events** - Realistic user interaction testing

### Test Structure

```
src/test/
├── setup.ts              # Test configuration
├── utils.tsx             # Testing utilities
├── mocks/                # API mocking
│   ├── server.ts         # MSW server setup
│   └── handlers.ts       # API request handlers
├── stores/               # Store tests
├── components/           # Component tests
├── integration/          # Integration tests
└── api.test.ts          # API layer tests
```

### Running Tests

```bash
# Watch mode (for development)
npm run test

# Single run (for CI)
npm run test:run

# With coverage
npm run test:coverage

# Specific test file
npm run test:run -- src/test/api.test.ts
```

### Test Coverage

- ✅ **API Layer**: Complete endpoint testing (19 tests)
- ✅ **Store Logic**: State management validation (7+ tests)
- ✅ **Components**: UI rendering and interaction tests
- ✅ **Integration**: End-to-end user workflows

## Project Structure

```
src/
├── components/           # React components
│   ├── MockRoutes.tsx   # Mock route management
│   ├── Config.tsx       # Configuration panel
│   ├── Home.tsx         # Dashboard/home page
│   ├── Logs.tsx         # Request logs viewer
│   └── OpenApiDocuments/ # OpenAPI document management
├── store/               # Zustand state stores
│   ├── useProckStore.ts # Mock routes & config store
│   └── useOpenApiStore.ts # OpenAPI documents store
├── network/             # API layer
│   └── api.ts          # HTTP client and API functions
├── helpers/             # Utility functions
├── assets/             # Static assets
└── test/               # Test suite
```

## Key Features

### Mock Route Management
- Create, edit, and delete mock routes
- Enable/disable individual routes
- JSON response editing with syntax highlighting
- HTTP method and status code configuration

### OpenAPI Integration
- Upload OpenAPI/Swagger specifications
- Auto-generate mock routes from OpenAPI documents
- View and manage OpenAPI documentation
- JSON viewer for OpenAPI specifications

### Real-time Updates
- Live updates via SignalR
- Request logging and monitoring
- Configuration changes reflected immediately

### Modern UI/UX
- Responsive Bootstrap design
- Accessible components and navigation
- Form validation and error handling
- Loading states and user feedback

## API Integration

The frontend communicates with the backend API using:

### HTTP Client (Axios)
- Centralized API configuration
- Request/response interceptors
- Error handling and retry logic
- Type-safe request/response interfaces

### State Management (Zustand)
- Lightweight, TypeScript-friendly stores
- Async data handling with loading/error states
- Optimistic updates for better UX
- Persistent state where needed

### Real-time Communication (SignalR)
- Live request monitoring
- Configuration change notifications
- Connection state management

## Development Guidelines

### Code Style
- ESLint + TypeScript for code quality
- Prettier for consistent formatting
- Component-driven development
- Custom hooks for reusable logic

### Testing Standards
- Test user behavior, not implementation
- Mock external dependencies (API, browser APIs)
- Comprehensive error scenario testing
- Accessibility testing where applicable

### Performance
- Code splitting with React.lazy
- Optimized bundle size with Vite
- Efficient re-rendering with proper dependencies
- Image optimization and lazy loading

## Deployment

### Production Build
```bash
npm run build
```

The build generates optimized static files in the `dist/` directory.

### Docker
The frontend is containerized with a multi-stage Dockerfile:
1. **Build stage**: Node.js for building the application
2. **Serve stage**: Nginx for serving static files

### Environment Configuration
- `VITE_API_BASE_URL` - Backend API base URL
- Additional environment variables can be added with `VITE_` prefix

## Browser Support

- Chrome/Edge 88+
- Firefox 87+
- Safari 14+
- Modern browsers with ES2020 support
