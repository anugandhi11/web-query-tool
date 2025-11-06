# Web Query Tool - Testing Guide

## 🧪 Testing Strategy

This document covers testing approaches to verify the WAF-friendly architecture works correctly.

---

## 1. Manual Testing

### ✅ Test 1: Base64 Encoding Verification

**Objective**: Verify SQL queries are encoded before transmission

**Steps**:
1. Start the application
2. Login with credentials
3. Open Browser DevTools (F12)
4. Go to Network tab
5. Navigate to Query Editor
6. Enter SQL: `SELECT * FROM employees LIMIT 10`
7. Click Execute
8. Find request to `/api/v1/query/execute`
9. Inspect Request Payload

**Expected Result**:
```json
{
  "queryData": "U0VMRUNUICogRlJPTSBlbXBsb3llZXMgTElNSVQgMTA=",
  "connectionId": "conn_xxx",
  "options": { "maxRows": 1000, "timeoutSeconds": 60 }
}
```

**Verification**:
```bash
# Decode to verify
echo "U0VMRUNUICogRlJPTSBlbXBsb3llZXMgTElNSVQgMTA=" | base64 -d
# Output: SELECT * FROM employees LIMIT 10
```

✅ **Pass**: `queryData` contains Base64 string (not raw SQL)
❌ **Fail**: Request contains raw SQL like `"sql": "SELECT..."`

---

### ✅ Test 2: Query Builder (No SQL in Request)

**Objective**: Verify query builder sends only structured data

**Steps**:
1. Use Query Builder feature (future implementation)
2. Select table: `employees`
3. Select columns: `id`, `name`, `email`
4. Add filter: `department = Engineering`
5. Execute query
6. Inspect network request

**Expected Result**:
```json
{
  "table": "employees",
  "columns": ["id", "name", "email"],
  "filters": [
    { "column": "department", "operator": "Equals", "value": "Engineering" }
  ]
}
```

✅ **Pass**: No SQL keywords in request body
❌ **Fail**: Contains SQL like `SELECT * FROM employees WHERE department = 'Engineering'`

---

### ✅ Test 3: SQL Injection Prevention

**Objective**: Verify backend properly handles SQL injection attempts

**Test Cases**:

#### Test 3.1: SQL Injection in Query
```sql
-- Attempt injection
SELECT * FROM users WHERE id = 1 OR 1=1;--
```

**Expected**:
- Query executes safely using PreparedStatement
- No unauthorized data access
- Audit log records the query

#### Test 3.2: SQL Injection in Table Name
```javascript
// Frontend: Try to inject via Query Builder
{
  "table": "employees; DROP TABLE users;--",
  "columns": ["*"]
}
```

**Expected**:
- Backend sanitizes identifier: `employeesDROPTABLEusers`
- Invalid table name error (not SQL injection)
- No tables dropped

✅ **Pass**: SQL injection attempts are neutralized
❌ **Fail**: Injection succeeds or causes SQL error revealing structure

---

### ✅ Test 4: Authentication & Authorization

**Test 4.1: Unauthenticated Access**
```bash
curl http://localhost:5001/api/v1/query/execute \
  -H "Content-Type: application/json" \
  -d '{"queryData":"U0VMRUNUIio=","connectionId":"conn_123"}'
```

**Expected**: 401 Unauthorized

**Test 4.2: Expired Token**
```bash
# Use an expired JWT token
curl http://localhost:5001/api/v1/query/execute \
  -H "Authorization: Bearer <expired_token>" \
  -d '{"queryData":"U0VMRUNUIio=","connectionId":"conn_123"}'
```

**Expected**: 401 Unauthorized

**Test 4.3: Access Other User's Connection**
```bash
# User A tries to use User B's connection
```

**Expected**: 403 Forbidden or Connection not found

---

## 2. Automated Testing

### Backend Unit Tests (C#)

Create `WebQueryTool.Tests` project:

```bash
cd backend
dotnet new xunit -n WebQueryTool.Tests
dotnet add WebQueryTool.Tests/WebQueryTool.Tests.csproj reference WebQueryTool.API/WebQueryTool.API.csproj
```

#### Test: Query Encoding Service

```csharp
[Fact]
public void EncodeQuery_ValidSQL_ReturnsBase64()
{
    // Arrange
    var service = new QueryEncodingService(Mock.Of<ILogger<QueryEncodingService>>());
    var sql = "SELECT * FROM users";

    // Act
    var encoded = service.Encode(sql);

    // Assert
    Assert.NotEmpty(encoded);
    Assert.NotEqual(sql, encoded);

    // Verify it's valid Base64
    var decoded = service.Decode(encoded);
    Assert.Equal(sql, decoded);
}

[Fact]
public void Decode_InvalidBase64_ThrowsException()
{
    // Arrange
    var service = new QueryEncodingService(Mock.Of<ILogger<QueryEncodingService>>());

    // Act & Assert
    Assert.Throws<ArgumentException>(() => service.Decode("not-valid-base64!@#"));
}
```

#### Test: SQL Sanitization

```csharp
[Theory]
[InlineData("employees", "employees")]
[InlineData("emp_data", "emp_data")]
[InlineData("employees; DROP TABLE users;--", "employeesDROPTABLEusers")]
[InlineData("table'; SELECT password FROM users;--", "tableSELECTpasswordFROMusers")]
public void SanitizeIdentifier_VariousInputs_RemovesSpecialChars(string input, string expected)
{
    // Arrange
    var service = new QueryExecutionService(...);

    // Act
    var result = service.SanitizeIdentifier(input);

    // Assert
    Assert.Equal(expected, result);
}
```

#### Test: Connection String Building

```csharp
[Fact]
public void BuildConnectionString_PostgreSQL_ReturnsCorrectFormat()
{
    // Arrange
    var service = new ConnectionPoolService(...);
    var config = new DatabaseConnection
    {
        Type = DatabaseType.PostgreSQL,
        Host = "localhost",
        Port = 5432,
        Database = "testdb",
        Username = "user",
        QueryTimeoutSeconds = 60
    };

    // Act
    var connectionString = service.BuildConnectionString(config, "password");

    // Assert
    Assert.Contains("Host=localhost", connectionString);
    Assert.Contains("Port=5432", connectionString);
    Assert.Contains("Database=testdb", connectionString);
    Assert.Contains("Username=user", connectionString);
    Assert.Contains("Password=password", connectionString);
}
```

### Frontend Unit Tests (Angular/Jasmine)

#### Test: Query Service Encoding

```typescript
// query.service.spec.ts
describe('QueryService', () => {
  let service: QueryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(QueryService);
  });

  it('should encode SQL query to Base64', () => {
    const sql = 'SELECT * FROM users';
    const encoded = service.encodeSqlQuery(sql);

    expect(encoded).toBeTruthy();
    expect(encoded).not.toEqual(sql);
    expect(encoded).toMatch(/^[A-Za-z0-9+/=]+$/); // Valid Base64 pattern
  });

  it('should decode Base64 back to SQL', () => {
    const sql = 'SELECT * FROM users';
    const encoded = service.encodeSqlQuery(sql);
    const decoded = service.decodeSqlQuery(encoded);

    expect(decoded).toEqual(sql);
  });

  it('should handle special SQL characters', () => {
    const sql = "SELECT * FROM users WHERE name = 'O''Brien'";
    const encoded = service.encodeSqlQuery(sql);
    const decoded = service.decodeSqlQuery(encoded);

    expect(decoded).toEqual(sql);
  });
});
```

### Integration Tests

#### Test: End-to-End Query Execution

```csharp
[Fact]
public async Task ExecuteEncodedQuery_ValidQuery_ReturnsResults()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    // Login to get token
    var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new {
        email = "admin@example.com",
        password = "Admin@123"
    });
    var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

    // Create connection
    var connectionResponse = await client.PostAsJsonAsync("/api/v1/connection", new {
        name = "Test DB",
        type = "PostgreSQL",
        host = "localhost",
        port = 5432,
        database = "testdb",
        username = "postgres",
        password = "postgres",
        queryTimeoutSeconds = 60
    }, new { Authorization = $"Bearer {loginData.Token}" });
    var connection = await connectionResponse.Content.ReadFromJsonAsync<ConnectionResponse>();

    // Execute query
    var sql = "SELECT 1 as test_value";
    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(sql));

    var queryResponse = await client.PostAsJsonAsync("/api/v1/query/execute", new {
        queryData = encoded,
        connectionId = connection.Id,
        options = new { maxRows = 1000, timeoutSeconds = 30 }
    }, new { Authorization = $"Bearer {loginData.Token}" });

    // Assert
    Assert.True(queryResponse.IsSuccessStatusCode);
    var result = await queryResponse.Content.ReadFromJsonAsync<QueryResult>();
    Assert.True(result.Success);
    Assert.Single(result.Rows);
    Assert.Equal(1, result.Rows[0][0]);
}
```

---

## 3. WAF Testing

### Test WAF Response (With Imperva)

**Setup**: Deploy application behind Imperva WAF in test environment

#### Test 3.1: Raw SQL Request (Should be blocked)

```bash
# Attempt to send raw SQL (this should be blocked by WAF)
curl -X POST https://your-app.com/api/test-raw-sql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "sql": "SELECT * FROM users WHERE department = '\''Engineering'\''"
  }'
```

**Expected**: 403 Forbidden (Imperva block page)

#### Test 3.2: Base64 Encoded SQL Request (Should be allowed)

```bash
# Encode SQL
SQL="SELECT * FROM users WHERE department = 'Engineering'"
ENCODED=$(echo -n "$SQL" | base64)

# Send encoded SQL
curl -X POST https://your-app.com/api/v1/query/execute \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"queryData\": \"$ENCODED\",
    \"connectionId\": \"conn_123\"
  }"
```

**Expected**: 200 OK with query results

#### Test 3.3: Query Builder Request (Should be allowed)

```bash
curl -X POST https://your-app.com/api/v1/query/build \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "table": "employees",
    "columns": ["id", "name"],
    "filters": [
      {"column": "department", "operator": "Equals", "value": "Engineering"}
    ]
  }'
```

**Expected**: 200 OK with query results

### Imperva Log Analysis

Check Imperva logs to verify:
1. Raw SQL requests are blocked (rule: SQL Injection)
2. Base64 encoded requests are allowed (no rule triggered)
3. Query builder requests are allowed (no SQL keywords detected)

---

## 4. Performance Testing

### Load Testing with k6

Install k6: https://k6.io/docs/getting-started/installation/

#### Test: Query Execution Performance

```javascript
// load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 10 },  // Ramp up to 10 users
    { duration: '1m', target: 10 },   // Stay at 10 users
    { duration: '30s', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<3000'], // 95% of requests under 3s
  },
};

const BASE_URL = 'http://localhost:5001';
let token = '';

export function setup() {
  // Login to get token
  const loginRes = http.post(`${BASE_URL}/api/v1/auth/login`, JSON.stringify({
    email: 'admin@example.com',
    password: 'Admin@123'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  const loginData = JSON.parse(loginRes.body);
  return { token: loginData.token, connectionId: 'conn_123' };
}

export default function (data) {
  const sql = 'SELECT * FROM employees LIMIT 100';
  const encoded = __ENV.BASE64_ENCODE ? btoa(sql) : sql;

  const payload = JSON.stringify({
    queryData: encoded,
    connectionId: data.connectionId,
  });

  const res = http.post(`${BASE_URL}/api/v1/query/execute`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${data.token}`,
    },
  });

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 3s': (r) => r.timings.duration < 3000,
    'has results': (r) => JSON.parse(r.body).rowCount > 0,
  });

  sleep(1);
}
```

Run test:
```bash
k6 run load-test.js
```

**Expected Results**:
- 95% of requests complete in < 3 seconds
- 0% error rate
- Consistent throughput under load

---

## 5. Security Testing

### OWASP Top 10 Checklist

#### A01: Broken Access Control
- ✅ Test: User can only access their own connections
- ✅ Test: Cannot access other user's query history
- ✅ Test: Role-based permissions enforced

#### A02: Cryptographic Failures
- ✅ Test: Passwords stored as BCrypt hash
- ✅ Test: Database credentials encrypted
- ✅ Test: JWT token signed with secret key
- ⚠️ Warning: Current password encryption is Base64 (not secure for production)

#### A03: Injection
- ✅ Test: SQL injection attempts fail (PreparedStatements)
- ✅ Test: Identifier sanitization works
- ✅ Test: No command injection possible

#### A04: Insecure Design
- ✅ Base64 encoding prevents WAF false positives
- ✅ Query builder eliminates SQL in transit
- ✅ Audit logging for all queries

#### A07: Identification and Authentication Failures
- ✅ Test: Strong password requirements (min 8 chars)
- ✅ Test: JWT token expiration enforced
- ✅ Test: Failed login attempts logged
- ❌ Missing: Account lockout after failed attempts (TODO)

### Penetration Testing

Use tools like:
- **Burp Suite**: For manual security testing
- **OWASP ZAP**: For automated vulnerability scanning
- **SQLMap**: To verify SQL injection protection

---

## 6. Test Automation (CI/CD)

### GitHub Actions Workflow

```yaml
# .github/workflows/test.yml
name: Test

on: [push, pull_request]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
        working-directory: ./backend
      - name: Build
        run: dotnet build --no-restore
        working-directory: ./backend
      - name: Test
        run: dotnet test --no-build --verbosity normal
        working-directory: ./backend

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      - name: Install dependencies
        run: npm ci
        working-directory: ./frontend
      - name: Run tests
        run: npm test -- --watch=false --browsers=ChromeHeadless
        working-directory: ./frontend
      - name: Build
        run: npm run build
        working-directory: ./frontend
```

---

## 7. Test Coverage Goals

### Backend (C#)
- **Target**: 80% code coverage
- **Critical paths**: 100% coverage
  - QueryExecutionService
  - AuthService
  - QueryEncodingService

### Frontend (Angular)
- **Target**: 70% code coverage
- **Critical services**: 90% coverage
  - QueryService
  - AuthService
  - ConnectionService

---

## 8. Testing Checklist Before Production

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] WAF testing completed (Base64 requests allowed)
- [ ] SQL injection attempts properly handled
- [ ] Authentication/authorization working
- [ ] Performance tests meet SLA (< 3s for typical queries)
- [ ] Load testing passed (50+ concurrent users)
- [ ] Security scan completed (OWASP ZAP)
- [ ] Penetration testing completed
- [ ] Audit logging verified
- [ ] Error handling tested
- [ ] Connection pooling working correctly
- [ ] Database migration tested

---

## 📊 Test Results Template

```markdown
# Test Execution Report

**Date**: 2025-01-06
**Environment**: QA
**Tester**: John Doe

## Summary
- Total Tests: 45
- Passed: 43
- Failed: 2
- Skipped: 0

## Critical Issues
1. **WAF Test Failure**: Raw SQL request not blocked in QA environment
   - **Root Cause**: WAF rules not configured in QA
   - **Fix**: Configure WAF rules or test in staging
   - **Status**: Deferred (WAF not needed for Base64 approach)

## Performance Results
- Average query execution: 1.2s
- 95th percentile: 2.8s
- Peak throughput: 150 queries/min

## Security Results
- SQL Injection: All attempts blocked ✅
- Authentication bypass: No vulnerabilities found ✅
- XSS: No vulnerabilities found ✅

## Recommendations
1. Implement account lockout after 5 failed login attempts
2. Add rate limiting for query execution (max 100/min per user)
3. Upgrade password encryption from Base64 to AWS Secrets Manager
```

---

**Happy Testing! 🧪**

For questions, contact the development team or create an issue in the repository.
