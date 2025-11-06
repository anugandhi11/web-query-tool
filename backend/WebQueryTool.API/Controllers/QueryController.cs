using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebQueryTool.API.Models.DTOs;
using WebQueryTool.API.Services;

namespace WebQueryTool.API.Controllers;

/// <summary>
/// Query execution endpoints - WAF-FRIENDLY IMPLEMENTATION
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class QueryController : ControllerBase
{
    private readonly IQueryExecutionService _queryService;
    private readonly ILogger<QueryController> _logger;

    public QueryController(
        IQueryExecutionService queryService,
        ILogger<QueryController> logger)
    {
        _queryService = queryService;
        _logger = logger;
    }

    private string GetUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("User ID not found in token");

    private string? GetClientIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>
    /// Execute a Base64-encoded SQL query
    /// WAF-FRIENDLY: SQL is encoded, WAF cannot pattern match
    /// </summary>
    /// <remarks>
    /// Example request:
    /// {
    ///   "queryData": "U0VMRUNUICogRlJPTSBlbXBsb3llZXMgTElNSVQgMTA=",
    ///   "connectionId": "conn_abc123",
    ///   "options": {
    ///     "maxRows": 1000,
    ///     "timeoutSeconds": 30
    ///   }
    /// }
    /// </remarks>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(QueryResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ExecuteQuery([FromBody] QueryExecuteRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var ipAddress = GetClientIpAddress();

            _logger.LogInformation("Executing encoded query for user {UserId} on connection {ConnectionId}",
                userId, request.ConnectionId);

            var result = await _queryService.ExecuteEncodedQueryAsync(request, userId, ipAddress);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query");
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Build and execute a SQL query from parameters
    /// WAF-FRIENDLY: No SQL in request, built server-side with parameterized queries
    /// </summary>
    /// <remarks>
    /// Example request:
    /// {
    ///   "connectionId": "conn_abc123",
    ///   "table": "employees",
    ///   "columns": ["id", "name", "email"],
    ///   "filters": [
    ///     { "column": "department", "operator": "Equals", "value": "Engineering" }
    ///   ],
    ///   "orderBy": { "column": "name", "direction": "Ascending" },
    ///   "limit": 100
    /// }
    /// </remarks>
    [HttpPost("build")]
    [ProducesResponseType(typeof(QueryResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> BuildAndExecuteQuery([FromBody] QueryBuilderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var ipAddress = GetClientIpAddress();

            _logger.LogInformation("Executing built query for user {UserId} on connection {ConnectionId}",
                userId, request.ConnectionId);

            var result = await _queryService.ExecuteBuiltQueryAsync(request, userId, ipAddress);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing built query");
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Get query history for the current user
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetQueryHistory([FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        var history = await _queryService.GetQueryHistoryAsync(userId, limit);

        // Don't return the full SQL query to the frontend for security
        var sanitizedHistory = history.Select(h => new
        {
            h.Id,
            h.ConnectionId,
            SqlPreview = h.SqlQuery.Length > 100 ? h.SqlQuery.Substring(0, 100) + "..." : h.SqlQuery,
            h.SubmissionType,
            h.ExecutedAt,
            h.ExecutionTimeMs,
            h.RowCount,
            h.Success,
            h.ErrorMessage
        });

        return Ok(sanitizedHistory);
    }
}
