#!/bin/bash

# Web Query Tool - Setup Script
# This script initializes the development environment

set -e

echo "=================================================="
echo "Web Query Tool - Setup Script"
echo "=================================================="
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

echo "✅ Docker and Docker Compose are installed"
echo ""

# Create necessary directories
echo "📁 Creating directories..."
mkdir -p backend/WebQueryTool.API/logs
mkdir -p frontend/dist

echo "✅ Directories created"
echo ""

# Check if .env file exists
if [ ! -f .env ]; then
    echo "📝 Creating .env file..."
    cat > .env << EOF
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
EOF
    echo "✅ .env file created"
else
    echo "✅ .env file already exists"
fi

echo ""
echo "=================================================="
echo "Starting services with Docker Compose..."
echo "=================================================="
echo ""

# Start Docker Compose
docker-compose up -d postgres

echo ""
echo "⏳ Waiting for PostgreSQL to be ready..."
sleep 10

# Run database migrations
echo ""
echo "📊 Running database migrations..."
docker exec -i webquerytool-postgres psql -U postgres -d webquerytool < backend/WebQueryTool.API/Migrations/20250106_InitialCreate.sql

echo ""
echo "✅ Database migrations completed"
echo ""

# Build and start all services
echo "🚀 Building and starting all services..."
docker-compose up -d --build

echo ""
echo "⏳ Waiting for services to start..."
sleep 15

echo ""
echo "=================================================="
echo "✅ Setup completed successfully!"
echo "=================================================="
echo ""
echo "Services are now running:"
echo "  🌐 Frontend:  http://localhost:80"
echo "  🔧 Backend:   http://localhost:5001"
echo "  📊 Swagger:   http://localhost:5001/swagger"
echo "  🗄️  PostgreSQL: localhost:5432"
echo ""
echo "Default credentials:"
echo "  Email:    admin@example.com"
echo "  Password: Admin@123"
echo ""
echo "To view logs:"
echo "  docker-compose logs -f"
echo ""
echo "To stop services:"
echo "  docker-compose down"
echo ""
echo "=================================================="
