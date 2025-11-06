# Web Query Tool API

A .NET 8 Web API for querying web pages and extracting content using CSS selectors.

## Features

- **HTML Content Extraction**: Extract HTML content from web pages
- **Text Extraction**: Get plain text content from web pages
- **Link Extraction**: Extract all links from a web page
- **Image Extraction**: Get all image URLs from a web page
- **Metadata Extraction**: Extract page metadata (title, meta tags, etc.)
- **CSS Selector Support**: Use CSS selectors to filter and target specific content

## Technology Stack

- **.NET 8**: Latest .NET framework
- **ASP.NET Core Web API**: For building RESTful APIs
- **AngleSharp**: HTML parsing and DOM manipulation
- **Swagger/OpenAPI**: API documentation
- **HtmlAgilityPack**: HTML parsing library

## API Endpoints

### POST /api/webquery/query
Generic query endpoint that accepts a JSON request body:
```json
{
  "url": "https://example.com",
  "selector": ".content",
  "queryType": "Text"
}
```

Query Types: `HtmlContent`, `Text`, `Links`, `Images`, `Metadata`

### GET /api/webquery/html
Get HTML content from a URL:
```
GET /api/webquery/html?url=https://example.com&selector=.content
```

### GET /api/webquery/text
Get text content from a URL:
```
GET /api/webquery/text?url=https://example.com&selector=.content
```

### GET /api/webquery/links
Get all links from a URL:
```
GET /api/webquery/links?url=https://example.com
```

### GET /api/webquery/images
Get all images from a URL:
```
GET /api/webquery/images?url=https://example.com
```

### GET /api/webquery/metadata
Get metadata from a URL:
```
GET /api/webquery/metadata?url=https://example.com
```

### GET /api/webquery/health
Health check endpoint

## Getting Started

### Prerequisites
- .NET 8 SDK

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd web-query-tool
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the API:
```bash
dotnet run --project src/WebQueryTool.Api
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`)

### Swagger UI

Access the Swagger UI at: `https://localhost:5001/swagger`

### Running with Docker

Build and run using Docker Compose:
```bash
docker-compose up --build
```

Or build and run manually:
```bash
docker build -t webquerytool-api .
docker run -p 5000:80 webquerytool-api
```

The API will be available at `http://localhost:5000`

### Running Tests

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Run tests in watch mode:
```bash
dotnet watch test --project tests/WebQueryTool.Api.Tests
```

## Project Structure

```
web-query-tool/
├── .github/
│   └── workflows/
│       └── dotnet.yml          # CI/CD workflow
├── src/
│   └── WebQueryTool.Api/
│       ├── Controllers/
│       │   └── WebQueryController.cs
│       ├── Models/
│       │   ├── WebQueryRequest.cs
│       │   └── WebQueryResponse.cs
│       ├── Services/
│       │   ├── IWebQueryService.cs
│       │   └── WebQueryService.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── WebQueryTool.Api.csproj
├── tests/
│   └── WebQueryTool.Api.Tests/
│       ├── Controllers/
│       │   └── WebQueryControllerTests.cs
│       ├── Services/
│       │   └── WebQueryServiceTests.cs
│       └── WebQueryTool.Api.Tests.csproj
├── Dockerfile                   # Docker build configuration
├── docker-compose.yml           # Docker Compose configuration
├── .editorconfig                # Code formatting rules
└── WebQueryTool.sln
```

## NuGet Packages Used

### API
- **Microsoft.AspNetCore.OpenApi** (8.0.0): OpenAPI support
- **Swashbuckle.AspNetCore** (6.5.0): Swagger UI
- **HtmlAgilityPack** (1.11.57): HTML parsing
- **AngleSharp** (1.1.2): Advanced HTML parsing and DOM manipulation

### Tests
- **xUnit** (2.6.2): Testing framework
- **Moq** (4.20.70): Mocking framework
- **FluentAssertions** (6.12.0): Fluent assertion library
- **Microsoft.NET.Test.Sdk** (17.8.0): .NET Test SDK

## CI/CD

The project includes GitHub Actions workflow that:
- Builds the project on every push and pull request
- Runs all unit tests
- Collects code coverage
- Publishes build artifacts
- Builds Docker image

## Example Usage

### Using cURL

Get text content from a webpage:
```bash
curl "http://localhost:5000/api/webquery/text?url=https://example.com"
```

Query with CSS selector:
```bash
curl "http://localhost:5000/api/webquery/text?url=https://example.com&selector=.main-content"
```

Post query with JSON:
```bash
curl -X POST http://localhost:5000/api/webquery/query \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://example.com",
    "selector": "article",
    "queryType": "Text"
  }'
```

### Using PowerShell

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/webquery/metadata?url=https://example.com"
```

## Development

### Code Style
The project uses `.editorconfig` for consistent code formatting. Most modern IDEs will automatically apply these rules.

### Adding New Features
1. Create feature branch from main
2. Implement feature with tests
3. Ensure all tests pass: `dotnet test`
4. Submit pull request

## License

MIT
