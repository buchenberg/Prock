# Backend Tests

This project contains comprehensive unit tests for the Prock backend API using xUnit, FluentAssertions, and AutoFixture.

## Overview

The test suite covers:
- **Endpoint Logic**: All API endpoints including mock routes, configuration, and proxy functionality
- **Data Layer**: Database context operations and entity behavior
- **Business Logic**: Core application logic and validation

## Test Structure

```
backend.Tests/
├── TestBase/                    # Test infrastructure and helpers
│   ├── AutoMoqDataAttribute.cs  # Custom AutoFixture attribute with domain customizations
│   ├── TestDbContext.cs         # In-memory database helpers
│   └── MockHttpContext.cs       # HttpContext mocking utilities
├── Endpoints/                   # API endpoint tests
│   ├── ProckEndpointsTests.cs   # Mock route management tests
│   ├── ConfigEndpointsTests.cs  # Configuration endpoint tests
│   └── ProxyEndpointsTests.cs   # Proxy logic tests
├── Data/                        # Data layer tests
│   └── ProckDbContextTests.cs   # Database context tests
└── GlobalUsings.cs              # Global using statements
```

## Key Testing Technologies

### xUnit
Primary testing framework with excellent .NET integration and parallel test execution.

### FluentAssertions
Provides expressive and readable assertions:
```csharp
result.Should().BeOfType<Ok<List<MockRouteDto>>>();
okResult.Value.Should().HaveCount(3);
routes.Should().AllSatisfy(r => r.RouteId.Should().NotBeEmpty());
```

### AutoFixture with AutoMoq
Automatically generates test data and mocks:
```csharp
[Theory, AutoMoqData]
public async Task CreateMockRoute_WithValidData_ReturnsCreated(
    MockRoute mockRoute,
    [Frozen] Mock<ILogger> loggerMock)
{
    // AutoFixture creates realistic test data
    // [Frozen] ensures the same mock is used throughout the test
}
```

### Custom Test Attributes

#### `[AutoMoqData]`
Custom attribute that:
- Configures AutoFixture with AutoMoq
- Sets up domain-specific customizations
- Ensures valid test data for entities

#### `[InlineAutoMoqData]`
For parameterized tests with both specific values and auto-generated data:
```csharp
[Theory]
[InlineAutoMoqData("GET")]
[InlineAutoMoqData("POST")]
public async Task TestWithSpecificMethods(string method, MockRoute route)
```

## Test Database Strategy

### In-Memory Testing
Uses Entity Framework's in-memory provider for fast, isolated tests:
```csharp
await using var context = TestDbContext.CreateInMemory();
// or with seeded data
await using var context = await TestDbContext.CreateSeededAsync(fixture);
```

### Benefits:
- **Fast**: No real database connection
- **Isolated**: Each test gets a fresh database
- **Deterministic**: Predictable test data

## Running Tests

### Basic Test Run
```bash
cd backend
dotnet test backend.Tests/
```

### With Coverage
```bash
# PowerShell
./run-tests.ps1 -Coverage

# Bash
./run-tests.sh --coverage
```

### Watch Mode (for development)
```bash
# PowerShell
./run-tests.ps1 -Watch

# Bash
./run-tests.sh --watch
```

### Filtered Tests
```bash
# Run only endpoint tests
./run-tests.sh --filter "ProckEndpointsTests"

# Run specific test method
./run-tests.sh --filter "CreateMockRoute_WithValidData_ReturnsCreated"
```

## Test Categories

### 1. Endpoint Tests
Test the actual endpoint logic extracted from minimal APIs:

**ProckEndpointsTests**
- Mock route CRUD operations
- HTTP method validation
- Route enabling/disabling
- JSON serialization/deserialization

**ConfigEndpointsTests**
- Configuration retrieval
- Upstream URL updates
- Default value handling

**ProxyEndpointsTests**
- Mock route matching (case-insensitive)
- Upstream forwarding
- Error handling
- SignalR notifications

### 2. Data Layer Tests
Test database operations and entity behavior:

**ProckDbContextTests**
- Configuration queries
- OpenAPI document management
- CRUD operations
- Query filtering

### 3. Test Data Management

#### Realistic Test Data
AutoFixture customizations ensure:
- Valid HTTP methods (`GET`, `POST`, `PUT`, `DELETE`, `PATCH`)
- Proper HTTP status codes (100-599)
- Valid URLs and paths
- Proper JSON serialization

#### Example Customization:
```csharp
fixture.Customize<MockRoute>(composer =>
    composer
        .With(x => x.Method, () => "GET") // Valid HTTP method
        .With(x => x.HttpStatusCode, () => 200) // Valid status code
        .With(x => x.Path, () => "/api/test") // Valid path
        .With(x => x.Mock, () => JsonSerializer.Serialize(new { test = "data" })));
```

## Best Practices

### 1. Test Naming
Use descriptive names that clearly indicate:
- **What** is being tested
- **When** (under what conditions)  
- **What should happen** (expected outcome)

```csharp
public async Task CreateMockRoute_WithValidData_ReturnsCreatedWithRoute()
public async Task GetMockRoute_WhenRouteDoesNotExist_ReturnsNotFound()
```

### 2. Arrange-Act-Assert Pattern
```csharp
[Theory, AutoMoqData]
public async Task TestMethod(TestData data)
{
    // Arrange
    await using var context = await TestDbContext.CreateWithEntitiesAsync(data);
    
    // Act
    var result = await methodUnderTest(context);
    
    // Assert
    result.Should().BeOfType<ExpectedType>();
}
```

### 3. Test Isolation
Each test is completely isolated:
- Fresh in-memory database
- Independent test data
- No shared state between tests

### 4. Mocking Strategy
- Mock external dependencies (IHttpForwarder, IHubContext)
- Use real database context (in-memory)
- Test actual business logic, not mocked behavior

## Coverage Goals

Target coverage areas:
- ✅ **Happy Path**: Normal operation scenarios
- ✅ **Error Conditions**: Invalid input, missing data
- ✅ **Edge Cases**: Boundary conditions, empty collections
- ✅ **Validation**: Input validation and business rules

## Continuous Integration

Tests are designed to run in CI/CD pipelines:
- No external dependencies
- Fast execution (in-memory database)
- Deterministic results
- Clear failure reporting

## Future Enhancements

Potential additions:
- Integration tests with real MongoDB
- Performance benchmarks
- Property-based testing with FsCheck
- Mutation testing
- API contract testing
