using Microsoft.AspNetCore.Mvc;
using WebQueryTool.Api.Models;
using WebQueryTool.Api.Services;

namespace WebQueryTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(
        IDatabaseService databaseService,
        IConnectionManager connectionManager,
        ILogger<DatabaseController> logger)
    {
        _databaseService = databaseService;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    /// <summary>
    /// Test a database connection without saving it
    /// </summary>
    [HttpPost("test-connection")]
    public async Task<ActionResult<TestConnectionResponse>> TestConnection([FromBody] TestConnectionRequest request)
    {
        _logger.LogInformation("Testing connection to {Provider} at {Host}:{Port}", request.Provider, request.Host, request.Port);
        var result = await _databaseService.TestConnectionAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Create a new database connection
    /// </summary>
    [HttpPost("connections")]
    public ActionResult<string> CreateConnection([FromBody] DatabaseConnection connection)
    {
        try
        {
            var connectionId = _connectionManager.AddConnection(connection);
            return Ok(new { connectionId, message = "Connection created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create connection");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all saved connections
    /// </summary>
    [HttpGet("connections")]
    public ActionResult<List<DatabaseConnection>> GetConnections()
    {
        var connections = _connectionManager.GetAllConnections();

        // Don't send passwords to the client
        foreach (var conn in connections)
        {
            conn.Password = "***";
        }

        return Ok(connections);
    }

    /// <summary>
    /// Get a specific connection by ID
    /// </summary>
    [HttpGet("connections/{connectionId}")]
    public ActionResult<DatabaseConnection> GetConnection(string connectionId)
    {
        var connection = _connectionManager.GetConnection(connectionId);
        if (connection == null)
        {
            return NotFound(new { error = "Connection not found" });
        }

        connection.Password = "***";
        return Ok(connection);
    }

    /// <summary>
    /// Update an existing connection
    /// </summary>
    [HttpPut("connections/{connectionId}")]
    public ActionResult UpdateConnection(string connectionId, [FromBody] DatabaseConnection connection)
    {
        connection.Id = connectionId;
        var success = _connectionManager.UpdateConnection(connection);

        if (!success)
        {
            return NotFound(new { error = "Connection not found" });
        }

        return Ok(new { message = "Connection updated successfully" });
    }

    /// <summary>
    /// Delete a connection
    /// </summary>
    [HttpDelete("connections/{connectionId}")]
    public ActionResult DeleteConnection(string connectionId)
    {
        var success = _connectionManager.RemoveConnection(connectionId);

        if (!success)
        {
            return NotFound(new { error = "Connection not found" });
        }

        return Ok(new { message = "Connection deleted successfully" });
    }

    /// <summary>
    /// Execute a SQL query
    /// </summary>
    [HttpPost("execute-query")]
    public async Task<ActionResult<ExecuteQueryResponse>> ExecuteQuery([FromBody] ExecuteQueryRequest request)
    {
        var connection = _connectionManager.GetConnection(request.ConnectionId);
        if (connection == null)
        {
            return NotFound(new { error = "Connection not found" });
        }

        _logger.LogInformation("Executing query on connection {ConnectionId}", request.ConnectionId);

        var result = await _databaseService.ExecuteQueryAsync(connection, request.Query, request.MaxRows);
        return Ok(result);
    }

    /// <summary>
    /// Get database schema (tables, views, columns)
    /// </summary>
    [HttpGet("connections/{connectionId}/schema")]
    public async Task<ActionResult<SchemaInfo>> GetSchema(string connectionId)
    {
        var connection = _connectionManager.GetConnection(connectionId);
        if (connection == null)
        {
            return NotFound(new { error = "Connection not found" });
        }

        try
        {
            var schema = await _databaseService.GetSchemaAsync(connection);
            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get schema for connection {ConnectionId}", connectionId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get list of databases
    /// </summary>
    [HttpGet("connections/{connectionId}/databases")]
    public async Task<ActionResult<List<string>>> GetDatabases(string connectionId)
    {
        var connection = _connectionManager.GetConnection(connectionId);
        if (connection == null)
        {
            return NotFound(new { error = "Connection not found" });
        }

        try
        {
            var databases = await _databaseService.GetDatabasesAsync(connection);
            return Ok(databases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get databases for connection {ConnectionId}", connectionId);
            return BadRequest(new { error = ex.Message });
        }
    }
}
