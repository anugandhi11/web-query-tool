using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebQueryTool.API.Models.DTOs;
using WebQueryTool.API.Services;

namespace WebQueryTool.API.Controllers;

/// <summary>
/// Database connection management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly IConnectionService _connectionService;
    private readonly ILogger<ConnectionController> _logger;

    public ConnectionController(
        IConnectionService connectionService,
        ILogger<ConnectionController> logger)
    {
        _connectionService = connectionService;
        _logger = logger;
    }

    private string GetUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("User ID not found in token");

    /// <summary>
    /// Get all connections for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConnectionResponse>), 200)]
    public async Task<IActionResult> GetConnections()
    {
        var userId = GetUserId();
        var connections = await _connectionService.GetConnectionsAsync(userId);
        return Ok(connections);
    }

    /// <summary>
    /// Get a specific connection
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConnectionResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConnection(string id)
    {
        var userId = GetUserId();
        var connection = await _connectionService.GetConnectionAsync(id, userId);

        if (connection == null)
        {
            return NotFound();
        }

        return Ok(connection);
    }

    /// <summary>
    /// Create a new database connection
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConnectionResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateConnection([FromBody] CreateConnectionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var connection = await _connectionService.CreateConnectionAsync(request, userId);

            _logger.LogInformation("Created connection {Name} for user {UserId}", connection.Name, userId);

            return CreatedAtAction(
                nameof(GetConnection),
                new { id = connection.Id },
                new ConnectionResponse
                {
                    Id = connection.Id,
                    Name = connection.Name,
                    Type = connection.Type,
                    Host = connection.Host,
                    Port = connection.Port,
                    Database = connection.Database,
                    Username = connection.Username,
                    IsActive = connection.IsActive,
                    CreatedAt = connection.CreatedAt,
                    QueryTimeoutSeconds = connection.QueryTimeoutSeconds
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing connection
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ConnectionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateConnection(string id, [FromBody] CreateConnectionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var connection = await _connectionService.UpdateConnectionAsync(id, request, userId);

            return Ok(connection);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a connection
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteConnection(string id)
    {
        try
        {
            var userId = GetUserId();
            await _connectionService.DeleteConnectionAsync(id, userId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test an existing connection
    /// </summary>
    [HttpPost("{id}/test")]
    [ProducesResponseType(typeof(ConnectionTestResult), 200)]
    public async Task<IActionResult> TestConnection(string id)
    {
        var userId = GetUserId();
        var result = await _connectionService.TestConnectionAsync(id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Test a connection before saving
    /// </summary>
    [HttpPost("test")]
    [ProducesResponseType(typeof(ConnectionTestResult), 200)]
    public async Task<IActionResult> TestNewConnection([FromBody] CreateConnectionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _connectionService.TestNewConnectionAsync(request);
        return Ok(result);
    }
}
