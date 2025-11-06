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

## Project Structure

```
web-query-tool/
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
└── WebQueryTool.sln
```

## NuGet Packages Used

- **Microsoft.AspNetCore.OpenApi** (8.0.0): OpenAPI support
- **Swashbuckle.AspNetCore** (6.5.0): Swagger UI
- **HtmlAgilityPack** (1.11.57): HTML parsing
- **AngleSharp** (1.1.2): Advanced HTML parsing and DOM manipulation

## License

MIT
