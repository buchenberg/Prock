# Prock Mocking Proxy

**Work in progress**

**Prock** is a developer tool that makes UI and API development easier by acting as a reverse proxy with powerful mocking features. It lets you forward requests to an upstream API (e.g., QA or Dev) and override any route with a custom mock response—no backend changes required.

---

## Features

- **Reverse Proxy:** Forwards all requests to your configured upstream API unless a mock is defined.
- **Mocking:** Easily create, update, and enable/disable JSON response mocks for any route and HTTP method.
- **OpenAPI Integration:** Upload OpenAPI specs and auto-generate mock routes for all paths.
- **UI:** Modern React Bootstrap interface for managing mocks and OpenAPI docs.
- **Persistence:** Mocks and OpenAPI docs are stored in MongoDB.
- **Easy Setup:** Run locally or in Docker with minimal configuration.

---

## How It Works

- Requests to any route not explicitly mocked are transparently forwarded to your upstream API.
- Mocks are defined by HTTP method and path. If a mock exists, Prock returns your custom JSON response (status code 200).
- OpenAPI documents can be uploaded and used to generate mock routes automatically.

---

## Quick Start

### 1. Run with Docker (Recommended)

1. Copy `appsettings.Docker.json.example` to `appsettings.Docker.json` and set your `UpstreamUrl`.
2. Copy `frontend/.env.example` to `frontend/.env`.
3. Run:
   ```sh
   docker-compose up
   ```
4. Visit [http://localhost:8080](http://localhost:8080)

### 2. Run Locally

1. Install [MongoDB Community Edition](https://www.mongodb.com/try/download/community).
2. Copy and configure `appsettings.json` as needed for your environment.
3. (Optional) Copy `frontend/.env.example` to `frontend/.env`.
4. Start backend:
   ```sh
   cd backend
   dotnet run
   ```
5. Start frontend:
   ```sh
   cd frontend
   npm install
   npm run dev
   ```
6. Visit the URL shown in the frontend console.

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
  "enabled": true,
  "method": "GET",
  "mock": "{\"hello\":\"mock\"}",
  "path": "/some/path",
  "routeId": "bAvQ077TS3aaDKm+T9yiDg=="
}
```

---

## Architecture

- **Backend:** .NET, [YARP](https://microsoft.github.io/reverse-proxy/) for proxying, MongoDB for storage. Implements **Repository Pattern** with DTOs for clean separation.
- **Frontend:** React + Vite + React Bootstrap.
- **Mocks:** Stored in MongoDB, identified by `routeId` (GUID/UUID).

---

## Alternatives

- [Tweak](https://chromewebstore.google.com/detail/tweak-mock-and-modify-htt/feahianecghpnipmhphmfgmpdodhcapi?hl=en) (Chrome extension)
- [Mock Server](https://github.com/mock-server/mockserver) (Java, feature-rich)

---

## Contributing

Prock is a work in progress! Suggestions, issues, and PRs are welcome.
