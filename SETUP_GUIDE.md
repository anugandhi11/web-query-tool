# Web Query Tool - Complete Setup Guide

## 🎯 Quick Start (Recommended)

The easiest way to get started is using Docker Compose:

### Linux/Mac:
```bash
chmod +x scripts/setup.sh
./scripts/setup.sh
```

### Windows (PowerShell as Administrator):
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\scripts\setup.ps1
```

This will:
1. ✅ Check Docker and Docker Compose installation
2. ✅ Create necessary directories
3. ✅ Generate .env configuration file
4. ✅ Start PostgreSQL database
5. ✅ Run database migrations
6. ✅ Build and start all services (Backend + Frontend)

After setup completes, access the application at:
- **Frontend**: http://localhost
- **Backend API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger

**Default Credentials:**
- Email: `admin@example.com`
- Password: `Admin@123`

---

## 📋 Prerequisites

### Required Software:
- **Docker**: Version 20.10 or higher
- **Docker Compose**: Version 2.0 or higher

### For Manual Development (Optional):
- **.NET 8.0 SDK**: For backend development
- **Node.js 20+**: For frontend development
- **PostgreSQL 16**: For local database

---

## 🐳 Docker Deployment (Production-Ready)

### 1. Clone the Repository
```bash
git clone <repository-url>
cd web-query-tool
```

### 2. Configure Environment Variables

Edit `.env` file (created by setup script or create manually):

```env
# Database
POSTGRES_DB=webquerytool
POSTGRES_USER=postgres
POSTGRES_PASSWORD=<CHANGE_THIS_IN_PRODUCTION>

# JWT Security
JWT_SECRET_KEY=<GENERATE_STRONG_KEY_MIN_32_CHARS>
JWT_ISSUER=WebQueryTool
JWT_AUDIENCE=WebQueryTool-Users
JWT_EXPIRATION_MINUTES=60

# Application URLs
BACKEND_URL=https://your-domain.com/api
FRONTEND_URL=https://your-domain.com
```

### 3. Start Services
```bash
docker-compose up -d
```

### 4. View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
```

### 5. Stop Services
```bash
docker-compose down

# To also remove volumes (⚠️ deletes database data)
docker-compose down -v
```

---

## 💻 Manual Development Setup

### Backend (C# ASP.NET Core)

#### 1. Install Dependencies
```bash
cd backend/WebQueryTool.API
dotnet restore
```

#### 2. Configure Connection String

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=webquerytool;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars-for-development",
    "Issuer": "WebQueryTool",
    "Audience": "WebQueryTool-Users",
    "ExpirationMinutes": 60
  }
}
```

#### 3. Run Database Migrations
```bash
# Ensure PostgreSQL is running
psql -U postgres -d webquerytool -f Migrations/20250106_InitialCreate.sql
```

#### 4. Run Backend
```bash
dotnet run
```

Backend will be available at `https://localhost:5001`

### Frontend (Angular 19)

#### 1. Install Dependencies
```bash
cd frontend
npm install
```

#### 2. Configure API Proxy

The `src/proxy.conf.json` file should point to your backend:
```json
{
  "/api": {
    "target": "https://localhost:5001",
    "secure": false,
    "changeOrigin": true
  }
}
```

#### 3. Run Frontend
```bash
npm start
```

Frontend will be available at `http://localhost:4200`

---

## 🧪 Testing the Application

### 1. Login
- Open http://localhost (Docker) or http://localhost:4200 (dev)
- Use default credentials:
  - Email: `admin@example.com`
  - Password: `Admin@123`

### 2. Create a Database Connection

Navigate to **Connections** and add a new connection:

**Example: Local PostgreSQL**
```
Name: My PostgreSQL
Type: PostgreSQL
Host: localhost
Port: 5432
Database: your_database
Username: your_user
Password: your_password
Timeout: 60 seconds
```

Click **Test Connection** to verify before saving.

### 3. Execute a Query

Go to **Query Editor**:
1. Select your connection from dropdown
2. Enter SQL query:
   ```sql
   SELECT * FROM your_table LIMIT 10;
   ```
3. Click **Execute** (or press Ctrl+Enter)
4. View results in the grid below

### 4. WAF-Friendly Testing

To verify Base64 encoding is working:

1. Open browser DevTools (F12) → Network tab
2. Execute a query
3. Find the request to `/api/v1/query/execute`
4. Inspect the request payload:
   ```json
   {
     "queryData": "U0VMRUNUICogRlJPTSB0YWJsZQ==",
     "connectionId": "conn_abc123"
   }
   ```
5. ✅ **SQL is Base64 encoded** - WAF cannot detect SQL keywords!

To decode and verify:
```bash
echo "U0VMRUNUICogRlJPTSB0YWJsZQ==" | base64 -d
# Output: SELECT * FROM table
```

---

## 🔒 Security Best Practices

### Production Deployment Checklist:

#### 1. Change Default Credentials
```bash
# Create a new admin user via API or update database directly
# Then disable the default admin@example.com account
```

#### 2. Use Strong JWT Secret
```bash
# Generate a secure random key (at least 32 characters)
openssl rand -base64 32
```

#### 3. Enable HTTPS
- Configure SSL certificates in nginx.conf
- Use Let's Encrypt for free SSL certificates

#### 4. Secure Database Credentials
- Use environment variables or secrets management
- Consider AWS Secrets Manager or HashiCorp Vault

#### 5. Configure Imperva WAF (If Applicable)

If you're using Imperva WAF, consider adding these rules for defense-in-depth:

```yaml
Rule Name: WebQueryTool_Trusted_Endpoints
Action: ALLOW (Skip SQL Injection rules for Base64 endpoints)
Conditions:
  - Path: /api/v1/query/*
  - Header: Authorization present (JWT)
  - Content-Type: application/json
  - Body contains: "queryData" (Base64 field indicator)
  - Source IP: [Your application server IPs]
```

**Note**: Our Base64 encoding strategy works WITHOUT Imperva configuration changes, but adding a custom rule provides additional security context.

---

## 📊 Database Schema

The application uses PostgreSQL to store:

### Tables:
- **Users**: Application users and authentication
- **DatabaseConnections**: Saved database connections (passwords encrypted)
- **QueryHistory**: Audit log of all executed queries

### Entity Relationship:
```
Users (1) ──< (N) DatabaseConnections
Users (1) ──< (N) QueryHistory
DatabaseConnections (1) ──< (N) QueryHistory
```

---

## 🔧 Troubleshooting

### Issue: Docker containers won't start
```bash
# Check if ports are already in use
sudo lsof -i :80    # Frontend
sudo lsof -i :5001  # Backend
sudo lsof -i :5432  # PostgreSQL

# Stop conflicting services or change ports in docker-compose.yml
```

### Issue: Backend can't connect to database
```bash
# Check PostgreSQL is running
docker-compose ps postgres

# View PostgreSQL logs
docker-compose logs postgres

# Verify connection string in backend logs
docker-compose logs backend | grep "Connection"
```

### Issue: Frontend shows "Failed to load" errors
```bash
# Check backend health
curl http://localhost:5001/health

# Verify API proxy configuration
cat frontend/src/proxy.conf.json

# Check browser console for CORS errors
# Open DevTools (F12) → Console tab
```

### Issue: 403 Forbidden from Imperva WAF
```bash
# Verify Base64 encoding is working
# Open DevTools → Network tab → Find /api/v1/query/execute request
# Request payload should show "queryData" with Base64 string

# If you see raw SQL in the request, the encoding is not working
# Check QueryService.encodeSqlQuery() implementation
```

### Issue: Database migration fails
```bash
# Manually run migration
docker exec -i webquerytool-postgres psql -U postgres -d webquerytool < backend/WebQueryTool.API/Migrations/20250106_InitialCreate.sql

# Check for existing tables
docker exec -it webquerytool-postgres psql -U postgres -d webquerytool -c "\dt"
```

---

## 📈 Performance Tuning

### Backend Optimizations:

1. **Connection Pooling**: Already configured in `ConnectionPoolService`
   - Min Pool Size: 1
   - Max Pool Size: 20 per connection

2. **Query Timeout**: Configurable per connection
   - Default: 60 seconds
   - Adjust in connection settings

3. **Result Set Limits**: Prevent memory issues
   - Default: 1000 rows max
   - Configurable in query options

### Frontend Optimizations:

1. **Monaco Editor**: Lazy-loaded for faster initial load
2. **Virtual Scrolling**: For large result sets (future enhancement)
3. **Gzip Compression**: Enabled in nginx.conf

---

## 🚀 Advanced Configuration

### Adding Support for New Database Types

To add support for Oracle, MongoDB, etc.:

1. **Backend**: Add NuGet package for database driver
2. **Update `DatabaseType` enum** in `Models/DatabaseConnection.cs`
3. **Implement connection string builder** in `ConnectionPoolService.cs`
4. **Add to frontend dropdown** in `connection-list.component.ts`

### Custom Query Templates

Create reusable query templates:

1. Add `QueryTemplate` table to database
2. Create `QueryTemplateService` in backend
3. Add template management UI in frontend

### WebSocket Support for Real-Time Updates

For long-running queries:

1. Configure SignalR in ASP.NET Core
2. Implement progress reporting
3. Add real-time status updates in Angular

---

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Angular 19 Documentation](https://angular.dev/)
- [Monaco Editor API](https://microsoft.github.io/monaco-editor/api/index.html)
- [Imperva WAF Documentation](https://docs.imperva.com/)

---

## 🤝 Support

For issues, questions, or contributions:
- Create an issue in the repository
- Contact: gajender@verisk.com
- Internal Wiki: [Add link]

---

**Built with ❤️ for Verisk Analytics Data Engineering Team**
