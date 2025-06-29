# ENSEK API Test Framework

A minimal C# test project for the ENSEK API, focusing on login and reset endpoints with basic logging and environment-based configuration.

## Features

- **Simple HTTP Client**: Built with HttpClient for essential API testing
- **Environment Configuration**: Uses `.env` files for configuration management
- **Basic Logging**: Console logging with Microsoft.Extensions.Logging
- **NUnit Testing**: Organized test methods for login, reset, and error handling
- **Error Handling**: Proper 401 Unauthorized detection and logging
- **Test Project Structure**: Properly configured as a test project (not console app)

## Project Structure

```
ApiTestFramework/
├── Core/
│   └── EnsekApiClient.cs      # ENSEK API client with login and reset
├── Models/
│   └── ApiModels.cs           # Data models for API requests/responses
├── Tests/
│   ├── EnsekLoginTests.cs     # Login endpoint tests (2 tests)
│   └── EnsekResetTests.cs     # Reset endpoint and workflow tests
├── Utilities/
│   └── EnvironmentLoader.cs   # Environment variable loader
├── .env                       # Environment configuration
├── .env.example              # Example environment configuration
└── ApiTestFramework.csproj   # Test project configuration
```

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 or VS Code

### Setup

1. Clone the repository
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Update configuration in `appsettings.json`:
   ```json
   {
   ```

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 or VS Code

### Setup

1. Clone/download the project
2. Copy `.env.example` to `.env` and configure:
   ```bash
   cp .env.example .env
   ```
3. Edit `.env` with your ENSEK API settings:
   ```env
   ENSEK_BASE_URL=https://qacandidatetest.ensek.io
   ENSEK_AUTH_TOKEN=12345
   ENSEK_USERNAME=test
   ENSEK_PASSWORD=testing
   ```
4. Restore packages and build:
   ```bash
   dotnet restore
   dotnet build
   ```

### Running Tests

Run all tests:

```bash
dotnet test
```

Run tests with verbose output:

```bash
dotnet test --verbosity normal
```

## Test Coverage

The framework includes the following test classes and methods:

### EnsekLoginTests.cs (2 tests)

- **Test_LoginEndpoint_ValidCredentials**: Tests ENSEK login with valid credentials
- **Test_LoginEndpoint_InvalidCredentials**: Tests error handling with invalid credentials

### EnsekResetTests.cs (3 tests)

- **Test_ShowConfiguration**: Displays current environment configuration
- **Test_ResetEndpoint**: Tests ENSEK reset functionality
- **Test_FullAutomationWorkflow**: Complete login → reset workflow

## Key Features

### Error Handling

- Proper 401 Unauthorized detection for invalid credentials
- Detailed error logging and response capture
- Graceful handling of network and parsing errors

### Environment Configuration

```env
ENSEK_BASE_URL=https://qacandidatetest.ensek.io
ENSEK_AUTH_TOKEN=12345
ENSEK_USERNAME=test
ENSEK_PASSWORD=testing
```

### Login Response Structure

```csharp
public class LoginApiResponse
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public LoginResponse? LoginData { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsUnauthorized => StatusCode == 401;
}
```

## Usage Examples

### Basic Login Test

```csharp
var loginRequest = new LoginRequest("test", "testing");
var response = await _ensekClient.LoginAsync(loginRequest);

if (response.IsSuccess)
{
    Console.WriteLine($"Login successful: {response.LoginData?.AccessToken}");
}
else if (response.IsUnauthorized)
{
    Console.WriteLine("401 Unauthorized - Invalid credentials");
}
```

### Reset Test Data

```csharp
var resetResponse = await _ensekClient.ResetTestDataAsync();
if (resetResponse != null)
{
    Console.WriteLine($"Reset successful: {resetResponse.Description}");
}
```

## Dependencies

- **Microsoft.NET.Test.Sdk** - Test framework support
- **NUnit** - Unit testing framework
- **NUnit3TestAdapter** - Test adapter for Visual Studio
- **Microsoft.Extensions.Logging** - Logging infrastructure
- **Microsoft.Extensions.Logging.Console** - Console logging

## Project History

This project was converted from a console application to a proper test project, focusing on:

- Simplified architecture with only essential endpoints (login and reset)
- Environment-based configuration instead of complex config systems
- Basic logging instead of advanced logging frameworks
- Standard test project structure instead of console app
- Proper error handling for API failures (especially 401 Unauthorized)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests as needed
5. Ensure all tests pass: `dotnet test`
6. Submit a pull request

## License

This project is provided as-is for educational and testing purposes.

- File logging for detailed analysis
- Configurable log levels
- Request/response logging for debugging

Log files are stored in the `logs/` directory with daily rotation.

## Best Practices

1. **Use the base test class**: Inherit from `ApiTestBase` for consistent setup
2. **Leverage test data builders**: Use `TestDataBuilder` for generating test data
3. **Assert meaningfully**: Use the provided assertion helpers
4. **Test error scenarios**: Don't just test happy paths
5. **Monitor performance**: Use response time assertions
6. **Clean up**: Reset test data when needed

## Contributing

1. Follow the established patterns
2. Add tests for new functionality
3. Update documentation
4. Use meaningful test names and descriptions

## Dependencies

- **NUnit**: Test framework
- **FluentAssertions**: Assertion library
- **RestSharp**: HTTP client
- **Newtonsoft.Json**: JSON serialization
- **Serilog**: Logging framework
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Logging**: Logging abstraction

## License

This project is licensed under the MIT License.
