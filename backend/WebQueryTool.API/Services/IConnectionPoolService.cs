using System.Data.Common;
using WebQueryTool.API.Models;

namespace WebQueryTool.API.Services;

/// <summary>
/// Service for managing database connection pools
/// </summary>
public interface IConnectionPoolService
{
    /// <summary>
    /// Get or create a connection from the pool
    /// </summary>
    Task<DbConnection> GetConnectionAsync(DatabaseConnection connectionConfig);

    /// <summary>
    /// Return a connection to the pool
    /// </summary>
    void ReturnConnection(DbConnection connection);

    /// <summary>
    /// Test a database connection
    /// </summary>
    Task<bool> TestConnectionAsync(DatabaseConnection connectionConfig);

    /// <summary>
    /// Build connection string from configuration
    /// </summary>
    string BuildConnectionString(DatabaseConnection connectionConfig, string decryptedPassword);

    /// <summary>
    /// Close all connections for a specific connection ID
    /// </summary>
    Task CloseConnectionsAsync(string connectionId);
}
