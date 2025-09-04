# Prock Mocking Proxy

**Prock** is a developer tool that makes UI and API development easier by acting as a reverse proxy with powerful mocking features. It lets you forward requests to an upstream API (e.g., QA or Dev) and override any route with a custom mock response—no backend changes required.

---

## Features

- **Reverse Proxy:** Forwards all requests to your configured upstream API unless a mock is defined.
- **Mocking:** Easily create, update, and enable/disable JSON response mocks for any route and HTTP method.
- **OpenAPI Integration:** Upload OpenAPI specs and auto-generate mock routes for all paths.
- **Modern UI:** React + TypeScript + Vite frontend with Bootstrap styling.
- **Persistent Storage:** All data stored in MariaDB for reliability and performance.
- **Containerized:** Full Docker support with docker-compose for easy deployment.
- **Comprehensive Testing:** Full test coverage for both backend and frontend.

---

## How It Works

- Requests to any route not explicitly mocked are transparently forwarded to your upstream API.
- Mocks are defined by HTTP method and path. If a mock exists, Prock returns your custom JSON response (status code 200).
- OpenAPI documents can be uploaded and used to generate mock routes automatically.

---

## Quick Start

### 1. Run with Docker (Recommended)

1. Copy `src/Backend.Api/appsettings.Docker.json.example` to `src/Backend.Api/appsettings.Docker.json` and configure your `UpstreamUrl`.
2. Run:
   ```sh
   docker-compose up
   ```
3. Visit [http://localhost:8080](http://localhost:8080)

The Docker setup includes:
- **MariaDB**: Database for storing mocks and OpenAPI documents
- **Backend API**: .NET 8 API server with YARP reverse proxy
- **Frontend**: React + TypeScript + Vite application

### 2. Run Locally

**Prerequisites:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [MariaDB](https://mariadb.org/download/) or [MySQL](https://dev.mysql.com/downloads/)

**Setup:**

1. **Database Setup:**
   ```sh
   # Install MariaDB and create database
   mysql -u root -p
   CREATE DATABASE mockula;
   ```

2. **Backend Setup:**
   ```sh
   cd src/Backend.Api
   # Copy and configure appsettings
   cp appsettings.json.example appsettings.json
   # Update connection string in appsettings.json
   dotnet run
   ```

3. **Frontend Setup:**
   ```sh
   cd src/frontend
   npm install
   npm run dev
   ```

4. Visit the URL shown in the frontend console (typically [http://localhost:5173](http://localhost:5173))

---

## Mocking a Route

1. Go to the **Mocks** tab in the UI.
2. Click the plus (+) button.
3. Select HTTP method, enter the path, and provide a JSON response.
4. Submit to create the mock.  
   Now, requests to that path will return your mock response!

---

## OpenAPI Integration

- Upload OpenAPI (Swagger) JSON/YAML files in the **OpenAPI** tab.
- Generate mock routes for all paths in the spec with one click.

---

## Example Mock Route Document

```json
{
  "id": 1,
  "routeId": "bAvQ077TS3aaDKm+T9yiDg==",
  "enabled": true,
  "method": "GET",
  "path": "/some/path",
  "httpStatusCode": 200,
  "mock": {
    "hello": "mock",
    "timestamp": "2024-01-01T00:00:00Z"
  }
}
```

---

## Architecture

### Backend (.NET 8)
- **API Layer**: Clean architecture with minimal APIs
- **Core Domain**: Business logic and entities
- **Infrastructure**: MariaDB data access with Entity Framework Core
- **Proxy**: [YARP](https://microsoft.github.io/reverse-proxy/) for high-performance reverse proxying
- **Real-time**: SignalR for live updates

### Frontend (React + TypeScript)
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite for fast development and optimized builds
- **UI**: React Bootstrap for responsive, accessible interface
- **State**: Zustand for lightweight state management
- **Testing**: Vitest + React Testing Library + MSW

### Database (MariaDB)
- **Mock Routes**: HTTP method, path, response data, and status
- **OpenAPI Documents**: Uploaded specs and generated documentation
- **Configuration**: Application settings and upstream URLs
- **Performance**: Optimized with proper indexing and relationships

---

## Development

### Project Structure

```
src/
├── Backend.Api/           # Main API application
├── Backend.Core/          # Business logic and domain models
├── Backend.Infrastructure/ # Data access and external services
├── Shared.Contracts/      # API contracts and DTOs
└── frontend/              # React TypeScript application
    ├── src/
    │   ├── components/    # React components
    │   ├── store/         # Zustand state management
    │   ├── network/       # API layer
    │   └── test/          # Test suite
    └── ...
```

### Running Tests

**Backend Tests:**
```sh
cd src/Backend.Api
dotnet test ../Backend.Tests/
```

**Frontend Tests:**
```sh
cd src/frontend
npm run test              # Watch mode
npm run test:run         # Single run
npm run test:coverage    # With coverage
```

### API Documentation

The API uses modern .NET minimal APIs with OpenAPI/Swagger documentation available at:
- **Local**: http://localhost:5001/swagger
- **Docker**: http://localhost:5001/swagger

### Key API Endpoints

- `GET /prock/api/mock-routes` - List all mock routes
- `POST /prock/api/mock-routes` - Create mock route
- `PUT /prock/api/mock-routes/{id}` - Update mock route
- `GET /prock/api/openapi/documents` - List OpenAPI documents
- `POST /prock/api/openapi/documents` - Upload OpenAPI document
- `GET /prock/api/config` - Get configuration
- `PUT /prock/api/config/upstream-url` - Update upstream URL

---

## Deployment

### Docker Production

```sh
# Build and run production containers
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Environment Variables

**Backend (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "MariaDbConnectionString": "server=localhost;user id=root;password=your_password;database=mockula"
  },
  "Prock": {
    "UpstreamUrl": "https://your-api.example.com"
  }
}
```

**Frontend (`.env`):**
```env
VITE_API_BASE_URL=http://localhost:5001
```

---

## Alternatives

- [Tweak](https://chromewebstore.google.com/detail/tweak-mock-and-modify-htt/feahianecghpnipmhphmfgmpdodhcapi?hl=en) (Chrome extension)
- [Mock Server](https://github.com/mock-server/mockserver) (Java, feature-rich)
- [WireMock](http://wiremock.org/) (Java, HTTP service virtualization)
- [Prism](https://stoplight.io/open-source/prism) (OpenAPI mock server)

---

## Contributing

We welcome contributions! Please see our contributing guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Test** your changes (both backend and frontend tests must pass)
4. **Commit** your changes (`git commit -m 'Add amazing feature'`)
5. **Push** to the branch (`git push origin feature/amazing-feature`)
6. **Open** a Pull Request

### Development Standards

- ✅ **Backend**: xUnit tests with FluentAssertions
- ✅ **Frontend**: Vitest tests with React Testing Library
- ✅ **Code Quality**: ESLint/Prettier for frontend, EditorConfig for backend
- ✅ **Documentation**: Update README and inline documentation
- ✅ **Type Safety**: Full TypeScript coverage in frontend
