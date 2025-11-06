# Web Query Tool - Angular Client

Angular 19 frontend for the Web Query Tool API.

## Features

- Modern Angular 19 standalone components
- Reactive forms with real-time validation
- Beautiful gradient UI with smooth animations
- Support for all query types:
  - HTML Content extraction
  - Text extraction
  - Link extraction
  - Image extraction
  - Metadata extraction
- CSS selector support for targeted queries
- Copy to clipboard functionality
- Responsive design
- API health check on startup

## Technology Stack

- **Angular 19**: Latest Angular framework with standalone components
- **TypeScript 5.6**: Type-safe development
- **SCSS**: Enhanced styling with variables and mixins
- **RxJS 7.8**: Reactive programming
- **HttpClient**: For API communication

## Prerequisites

- Node.js 18+ and npm
- The .NET backend API running (see main README)

## Installation

1. Navigate to the client directory:
```bash
cd client
```

2. Install dependencies:
```bash
npm install
```

## Development Server

Run the development server:
```bash
npm start
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Build

Build the project for production:
```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Running Tests

Run unit tests:
```bash
npm test
```

## Configuration

Update the API URL in the environment files:

- **Development**: `src/environments/environment.ts`
- **Production**: `src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
```

## Project Structure

```
client/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   └── query-form/        # Main query form component
│   │   ├── models/
│   │   │   └── web-query.model.ts # TypeScript interfaces
│   │   ├── services/
│   │   │   └── web-query.service.ts # API service
│   │   ├── app.component.ts       # Root component
│   │   ├── app.config.ts          # App configuration
│   │   └── app.routes.ts          # Routing configuration
│   ├── environments/              # Environment configs
│   ├── index.html                 # HTML entry point
│   ├── main.ts                    # Bootstrap file
│   └── styles.scss                # Global styles
├── angular.json                   # Angular CLI config
├── package.json                   # Dependencies
└── tsconfig.json                  # TypeScript config
```

## Usage

1. **Enter a URL**: Type the web page URL you want to query
2. **Select Query Type**: Choose what you want to extract:
   - HTML Content: Get raw HTML
   - Text: Get plain text content
   - Links: Extract all links
   - Images: Extract all images
   - Metadata: Get page metadata (title, meta tags)
3. **Add CSS Selector** (optional): Target specific elements
4. **Click Query**: Fetch and display results
5. **Copy Results**: Use the copy button to copy results to clipboard

## Features in Detail

### Query Types

- **HTML Content**: Returns the HTML markup of the page or selected elements
- **Text Content**: Extracts and returns plain text without HTML tags
- **Links**: Finds all anchor tags and returns their href attributes
- **Images**: Finds all image tags and returns their src attributes
- **Metadata**: Extracts page title and all meta tags

### CSS Selectors

You can use any valid CSS selector:
- `.classname` - Select by class
- `#id` - Select by ID
- `article` - Select by tag name
- `.content p` - Select nested elements
- `[data-attribute]` - Select by attribute

### API Integration

The service automatically communicates with the .NET backend API. Make sure the backend is running before starting the Angular app.

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Contributing

1. Create a feature branch
2. Make your changes
3. Run tests: `npm test`
4. Build: `npm run build`
5. Submit a pull request

## License

MIT
