using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using WebQueryTool.API.Data;
using WebQueryTool.API.Models;
using WebQueryTool.API.Models.DTOs;

namespace WebQueryTool.API.Services;

/// <summary>
/// Implementation of query execution service
/// CRITICAL: This service implements WAF-friendly query execution
/// </summary>
public class QueryExecutionService : IQueryExecutionService
{
    private readonly ApplicationDbContext _context;
    private readonly IConnectionPoolService _connectionPool;
    private readonly IQueryEncodingService _encodingService;
    private readonly ILogger<QueryExecutionService> _logger;
    private readonly IConfiguration _configuration;

    public QueryExecutionService(
        ApplicationDbContext context,
        IConnectionPoolService connectionPool,
        IQueryEncodingService encodingService,
        ILogger<QueryExecutionService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _connectionPool = connectionPool;
        _encodingService = encodingService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Execute Base64-encoded query
    /// WAF-FRIENDLY: SQL is encoded in transit, decoded on backend
    /// </summary>
    public async Task<QueryResult> ExecuteEncodedQueryAsync(
        QueryExecuteRequest request,
        string userId,
        string? ipAddress = null)
    {
        try
        {
            // Decode the Base64-encoded SQL
            var sql = _encodingService.Decode(request.QueryData);

            _logger.LogInformation("Executing encoded query for user {UserId} on connection {ConnectionId}",
                userId, request.ConnectionId);

            // Execute the decoded SQL
            return await ExecuteDirectQueryAsync(
                sql,
                request.ConnectionId,
                userId,
                request.Options,
                ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing encoded query");
            return new QueryResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Build and execute query from parameters
    /// WAF-FRIENDLY: No SQL in HTTP request, built server-side
    /// </summary>
    public async Task<QueryResult> ExecuteBuiltQueryAsync(
        QueryBuilderRequest request,
        string userId,
        string? ipAddress = null)
    {
        try
        {
            // Build SQL from parameters
            var (sql, parameters) = BuildSqlQuery(request);

            _logger.LogInformation("Executing built query for user {UserId} on connection {ConnectionId}",
                userId, request.ConnectionId);

            // Get connection
            var connectionConfig = await GetConnectionConfigAsync(request.ConnectionId, userId);
            using var connection = await _connectionPool.GetConnectionAsync(connectionConfig);

            // Execute with parameters (SQL injection safe)
            var result = await ExecuteQueryWithParametersAsync(
                connection,
                sql,
                parameters,
                request.Limit,
                connectionConfig.QueryTimeoutSeconds);

            // Log to audit
            await LogQueryAsync(
                userId,
                request.ConnectionId,
                sql,
                QuerySubmissionType.QueryBuilder,
                result,
                ipAddress);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing built query");
            return new QueryResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Execute SQL query directly
    /// WARNING: Should be disabled in production if not using Base64 encoding
    /// </summary>
    public async Task<QueryResult> ExecuteDirectQueryAsync(
        string sql,
        string connectionId,
        string userId,
        QueryExecutionOptions? options = null,
        string? ipAddress = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validate query
            ValidateQuery(sql);

            // Get connection configuration
            var connectionConfig = await GetConnectionConfigAsync(connectionId, userId);

            // Get database connection
            using var connection = await _connectionPool.GetConnectionAsync(connectionConfig);

            // Set execution options
            options ??= new QueryExecutionOptions();
            var timeout = Math.Min(options.TimeoutSeconds, connectionConfig.QueryTimeoutSeconds);

            // Execute query
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = timeout;
            command.CommandType = CommandType.Text;

            using var reader = await command.ExecuteReaderAsync();

            // Read results
            var result = await ReadQueryResultsAsync(reader, options.MaxRows, options.IncludeMetadata);

            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Success = true;

            // Log to audit
            await LogQueryAsync(
                userId,
                connectionId,
                sql,
                QuerySubmissionType.EncodedQuery,
                result,
                ipAddress);

            _logger.LogInformation("Query executed successfully in {Time}ms, returned {RowCount} rows",
                result.ExecutionTimeMs, result.RowCount);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Error executing query");

            // Log failed query
            await LogQueryAsync(
                userId,
                connectionId,
                sql,
                QuerySubmissionType.EncodedQuery,
                new QueryResult { Success = false, ErrorMessage = ex.Message },
                ipAddress);

            return new QueryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<List<QueryHistory>> GetQueryHistoryAsync(string userId, int limit = 50)
    {
        return await _context.QueryHistory
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.ExecutedAt)
            .Take(limit)
            .ToListAsync();
    }

    private async Task<QueryResult> ReadQueryResultsAsync(DbDataReader reader, int maxRows, bool includeMetadata)
    {
        var result = new QueryResult();

        // Get column names
        for (int i = 0; i < reader.FieldCount; i++)
        {
            result.Columns.Add(reader.GetName(i));
        }

        // Get column metadata if requested
        if (includeMetadata)
        {
            result.ColumnMetadata = new List<ColumnMetadata>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.ColumnMetadata.Add(new ColumnMetadata
                {
                    Name = reader.GetName(i),
                    DataType = reader.GetDataTypeName(i),
                    IsNullable = true // Would need schema query for accurate value
                });
            }
        }

        // Read rows
        int rowCount = 0;
        while (await reader.ReadAsync() && rowCount < maxRows)
        {
            var row = new List<object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
            }
            result.Rows.Add(row);
            rowCount++;
        }

        result.RowCount = rowCount;
        result.HasMoreRows = await reader.ReadAsync(); // Check if there are more rows

        return result;
    }

    private async Task<QueryResult> ExecuteQueryWithParametersAsync(
        DbConnection connection,
        string sql,
        Dictionary<string, object?> parameters,
        int maxRows,
        int timeoutSeconds)
    {
        var stopwatch = Stopwatch.StartNew();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = timeoutSeconds;

        // Add parameters (SQL injection safe)
        foreach (var param in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = param.Key;
            parameter.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        using var reader = await command.ExecuteReaderAsync();
        var result = await ReadQueryResultsAsync(reader, maxRows, true);

        stopwatch.Stop();
        result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        result.Success = true;

        return result;
    }

    /// <summary>
    /// Build SQL query from QueryBuilderRequest
    /// CRITICAL: Uses parameterized queries to prevent SQL injection
    /// </summary>
    private (string sql, Dictionary<string, object?> parameters) BuildSqlQuery(QueryBuilderRequest request)
    {
        var sql = new StringBuilder();
        var parameters = new Dictionary<string, object?>();

        // Validate and sanitize identifiers
        var tableName = SanitizeIdentifier(request.Table);
        var columns = request.Columns.Any()
            ? string.Join(", ", request.Columns.Select(SanitizeIdentifier))
            : "*";

        // Build SELECT clause
        sql.Append($"SELECT {columns} FROM {tableName}");

        // Build WHERE clause with parameters
        if (request.Filters.Any())
        {
            sql.Append(" WHERE ");
            var filterClauses = new List<string>();

            for (int i = 0; i < request.Filters.Count; i++)
            {
                var filter = request.Filters[i];
                var paramName = $"@param{i}";
                var column = SanitizeIdentifier(filter.Column);

                filterClauses.Add(filter.Operator switch
                {
                    FilterOperator.Equals => $"{column} = {paramName}",
                    FilterOperator.NotEquals => $"{column} <> {paramName}",
                    FilterOperator.GreaterThan => $"{column} > {paramName}",
                    FilterOperator.GreaterThanOrEqual => $"{column} >= {paramName}",
                    FilterOperator.LessThan => $"{column} < {paramName}",
                    FilterOperator.LessThanOrEqual => $"{column} <= {paramName}",
                    FilterOperator.Like => $"{column} LIKE {paramName}",
                    FilterOperator.NotLike => $"{column} NOT LIKE {paramName}",
                    FilterOperator.IsNull => $"{column} IS NULL",
                    FilterOperator.IsNotNull => $"{column} IS NOT NULL",
                    _ => throw new NotSupportedException($"Operator {filter.Operator} not supported")
                });

                if (filter.Operator != FilterOperator.IsNull && filter.Operator != FilterOperator.IsNotNull)
                {
                    parameters[paramName] = filter.Value;
                }
            }

            sql.Append(string.Join(" AND ", filterClauses));
        }

        // Build ORDER BY clause
        if (request.OrderBy != null)
        {
            var orderColumn = SanitizeIdentifier(request.OrderBy.Column);
            var direction = request.OrderBy.Direction == SortDirection.Ascending ? "ASC" : "DESC";
            sql.Append($" ORDER BY {orderColumn} {direction}");
        }

        // Add LIMIT and OFFSET
        sql.Append($" LIMIT {request.Limit} OFFSET {request.Offset}");

        return (sql.ToString(), parameters);
    }

    /// <summary>
    /// Sanitize SQL identifier (table/column name)
    /// CRITICAL: Prevents SQL injection in identifiers
    /// </summary>
    private string SanitizeIdentifier(string identifier)
    {
        // Remove any characters that are not alphanumeric or underscore
        var sanitized = new string(identifier.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

        if (string.IsNullOrEmpty(sanitized))
        {
            throw new ArgumentException($"Invalid identifier: {identifier}");
        }

        return sanitized;
    }

    private void ValidateQuery(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL query cannot be empty");
        }

        var maxLength = _configuration.GetValue<int>("Security:MaxQueryLength", 100000);
        if (sql.Length > maxLength)
        {
            throw new ArgumentException($"SQL query exceeds maximum length of {maxLength} characters");
        }

        // Additional validation can be added here
        // For example: blocking DDL statements, checking for dangerous keywords, etc.
    }

    private async Task<DatabaseConnection> GetConnectionConfigAsync(string connectionId, string userId)
    {
        var connection = await _context.DatabaseConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId && c.IsActive);

        if (connection == null)
        {
            throw new UnauthorizedAccessException("Connection not found or access denied");
        }

        return connection;
    }

    private async Task LogQueryAsync(
        string userId,
        string connectionId,
        string sql,
        QuerySubmissionType submissionType,
        QueryResult result,
        string? ipAddress)
    {
        try
        {
            var history = new QueryHistory
            {
                UserId = userId,
                ConnectionId = connectionId,
                SqlQuery = sql,
                SubmissionType = submissionType,
                ExecutedAt = DateTime.UtcNow,
                ExecutionTimeMs = result.ExecutionTimeMs,
                RowCount = result.RowCount,
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                IpAddress = ipAddress
            };

            _context.QueryHistory.Add(history);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log query execution");
            // Don't fail the query if logging fails
        }
    }
}
