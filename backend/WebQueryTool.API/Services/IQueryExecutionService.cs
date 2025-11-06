using WebQueryTool.API.Models;
using WebQueryTool.API.Models.DTOs;

namespace WebQueryTool.API.Services;

/// <summary>
/// Service for executing SQL queries
/// </summary>
public interface IQueryExecutionService
{
    /// <summary>
    /// Execute a Base64-encoded SQL query (WAF-FRIENDLY)
    /// </summary>
    Task<QueryResult> ExecuteEncodedQueryAsync(
        QueryExecuteRequest request,
        string userId,
        string? ipAddress = null);

    /// <summary>
    /// Build and execute a query from parameters (WAF-FRIENDLY - No SQL in request)
    /// </summary>
    Task<QueryResult> ExecuteBuiltQueryAsync(
        QueryBuilderRequest request,
        string userId,
        string? ipAddress = null);

    /// <summary>
    /// Execute a raw SQL query directly (SHOULD BE DISABLED IN PRODUCTION)
    /// </summary>
    Task<QueryResult> ExecuteDirectQueryAsync(
        string sql,
        string connectionId,
        string userId,
        QueryExecutionOptions? options = null,
        string? ipAddress = null);

    /// <summary>
    /// Get query history for a user
    /// </summary>
    Task<List<QueryHistory>> GetQueryHistoryAsync(string userId, int limit = 50);
}
