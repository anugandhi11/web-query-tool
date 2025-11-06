using WebQueryTool.API.Models;
using WebQueryTool.API.Models.DTOs;

namespace WebQueryTool.API.Services;

/// <summary>
/// Service for managing database connections
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Create a new connection
    /// </summary>
    Task<DatabaseConnection> CreateConnectionAsync(CreateConnectionRequest request, string userId);

    /// <summary>
    /// Get all connections for a user
    /// </summary>
    Task<List<ConnectionResponse>> GetConnectionsAsync(string userId);

    /// <summary>
    /// Get a specific connection
    /// </summary>
    Task<ConnectionResponse?> GetConnectionAsync(string connectionId, string userId);

    /// <summary>
    /// Update a connection
    /// </summary>
    Task<DatabaseConnection> UpdateConnectionAsync(string connectionId, CreateConnectionRequest request, string userId);

    /// <summary>
    /// Delete a connection
    /// </summary>
    Task DeleteConnectionAsync(string connectionId, string userId);

    /// <summary>
    /// Test a connection
    /// </summary>
    Task<ConnectionTestResult> TestConnectionAsync(string connectionId, string userId);

    /// <summary>
    /// Test a connection before saving
    /// </summary>
    Task<ConnectionTestResult> TestNewConnectionAsync(CreateConnectionRequest request);
}
