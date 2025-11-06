# Web Query Tool - Setup Script (Windows PowerShell)
# This script initializes the development environment

$ErrorActionPreference = "Stop"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Web Query Tool - Setup Script" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is installed
$dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerInstalled) {
    Write-Host "❌ Docker is not installed. Please install Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Check if Docker Compose is installed
$dockerComposeInstalled = Get-Command docker-compose -ErrorAction SilentlyContinue
if (-not $dockerComposeInstalled) {
    Write-Host "❌ Docker Compose is not installed. Please install Docker Compose first." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Docker and Docker Compose are installed" -ForegroundColor Green
Write-Host ""

# Create necessary directories
Write-Host "📁 Creating directories..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "backend/WebQueryTool.API/logs" | Out-Null
New-Item -ItemType Directory -Force -Path "frontend/dist" | Out-Null

Write-Host "✅ Directories created" -ForegroundColor Green
Write-Host ""

# Check if .env file exists
if (-not (Test-Path .env)) {
    Write-Host "📝 Creating .env file..." -ForegroundColor Yellow
    @"
# Database Configuration
POSTGRES_DB=webquerytool
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

# JWT Configuration
JWT_SECRET_KEY=your-super-secret-key-min-32-characters-long-change-in-production
JWT_ISSUER=WebQueryTool
JWT_AUDIENCE=WebQueryTool-Users
JWT_EXPIRATION_MINUTES=60

# Application URLs
BACKEND_URL=http://localhost:5001
FRONTEND_URL=http://localhost:80
"@ | Out-File -FilePath .env -Encoding UTF8
    Write-Host "✅ .env file created" -ForegroundColor Green
} else {
    Write-Host "✅ .env file already exists" -ForegroundColor Green
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Starting services with Docker Compose..." -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Start PostgreSQL first
docker-compose up -d postgres

Write-Host ""
Write-Host "⏳ Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Run database migrations
Write-Host ""
Write-Host "📊 Running database migrations..." -ForegroundColor Yellow
Get-Content backend/WebQueryTool.API/Migrations/20250106_InitialCreate.sql | docker exec -i webquerytool-postgres psql -U postgres -d webquerytool

Write-Host ""
Write-Host "✅ Database migrations completed" -ForegroundColor Green
Write-Host ""

# Build and start all services
Write-Host "🚀 Building and starting all services..." -ForegroundColor Yellow
docker-compose up -d --build

Write-Host ""
Write-Host "⏳ Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "✅ Setup completed successfully!" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services are now running:" -ForegroundColor Yellow
Write-Host "  🌐 Frontend:  http://localhost:80"
Write-Host "  🔧 Backend:   http://localhost:5001"
Write-Host "  📊 Swagger:   http://localhost:5001/swagger"
Write-Host "  🗄️  PostgreSQL: localhost:5432"
Write-Host ""
Write-Host "Default credentials:" -ForegroundColor Yellow
Write-Host "  Email:    admin@example.com"
Write-Host "  Password: Admin@123"
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Yellow
Write-Host "  docker-compose logs -f"
Write-Host ""
Write-Host "To stop services:" -ForegroundColor Yellow
Write-Host "  docker-compose down"
Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
