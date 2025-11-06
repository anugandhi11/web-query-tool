using System.Data.Common;
using System.Collections.Concurrent;
using Npgsql;
using MySqlConnector;
using Microsoft.Data.SqlClient;
using WebQueryTool.API.Models;

namespace WebQueryTool.API.Services;

/// <summary>
/// Implementation of connection pool service
/// Manages database connections for multiple database types
/// </summary>
public class ConnectionPoolService : IConnectionPoolService
{
    private readonly ILogger<ConnectionPoolService> _logger;
    private readonly ConcurrentDictionary<string, string> _connectionStrings = new();

    public ConnectionPoolService(ILogger<ConnectionPoolService> logger)
    {
        _logger = logger;
    }

    public async Task<DbConnection> GetConnectionAsync(DatabaseConnection connectionConfig)
    {
        var connectionString = BuildConnectionString(connectionConfig, DecryptPassword(connectionConfig.EncryptedPassword));

        DbConnection connection = connectionConfig.Type switch
        {
            DatabaseType.PostgreSQL => new NpgsqlConnection(connectionString),
            DatabaseType.MySQL => new MySqlConnection(connectionString),
            DatabaseType.SQLServer => new SqlConnection(connectionString),
            DatabaseType.Redshift => new NpgsqlConnection(connectionString), // Redshift uses PostgreSQL protocol
            _ => throw new NotSupportedException($"Database type {connectionConfig.Type} is not supported")
        };

        try
        {
            await connection.OpenAsync();
            _logger.LogDebug("Opened connection to {Type} database: {Name}", connectionConfig.Type, connectionConfig.Name);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open connection to {Type} database: {Name}", connectionConfig.Type, connectionConfig.Name);
            connection.Dispose();
            throw;
        }
    }

    public void ReturnConnection(DbConnection connection)
    {
        try
        {
            connection.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing connection");
        }
    }

    public async Task<bool> TestConnectionAsync(DatabaseConnection connectionConfig)
    {
        try
        {
            using var connection = await GetConnectionAsync(connectionConfig);
            return connection.State == System.Data.ConnectionState.Open;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed for {Name}", connectionConfig.Name);
            return false;
        }
    }

    public string BuildConnectionString(DatabaseConnection connectionConfig, string decryptedPassword)
    {
        return connectionConfig.Type switch
        {
            DatabaseType.PostgreSQL => BuildPostgreSqlConnectionString(connectionConfig, decryptedPassword),
            DatabaseType.MySQL => BuildMySqlConnectionString(connectionConfig, decryptedPassword),
            DatabaseType.SQLServer => BuildSqlServerConnectionString(connectionConfig, decryptedPassword),
            DatabaseType.Redshift => BuildRedshiftConnectionString(connectionConfig, decryptedPassword),
            _ => throw new NotSupportedException($"Database type {connectionConfig.Type} is not supported")
        };
    }

    public Task CloseConnectionsAsync(string connectionId)
    {
        // Connection pooling is handled by ADO.NET providers
        // Remove cached connection string
        _connectionStrings.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    private string BuildPostgreSqlConnectionString(DatabaseConnection config, string password)
    {
        return $"Host={config.Host};Port={config.Port};Database={config.Database};" +
               $"Username={config.Username};Password={password};" +
               $"Timeout={config.QueryTimeoutSeconds};Pooling=true;MinPoolSize=1;MaxPoolSize=20;";
    }

    private string BuildMySqlConnectionString(DatabaseConnection config, string password)
    {
        return $"Server={config.Host};Port={config.Port};Database={config.Database};" +
               $"User={config.Username};Password={password};" +
               $"ConnectionTimeout={config.QueryTimeoutSeconds};Pooling=true;MinimumPoolSize=1;MaximumPoolSize=20;";
    }

    private string BuildSqlServerConnectionString(DatabaseConnection config, string password)
    {
        return $"Server={config.Host},{config.Port};Database={config.Database};" +
               $"User Id={config.Username};Password={password};" +
               $"Connection Timeout={config.QueryTimeoutSeconds};Pooling=true;Min Pool Size=1;Max Pool Size=20;" +
               $"Encrypt=True;TrustServerCertificate=False;";
    }

    private string BuildRedshiftConnectionString(DatabaseConnection config, string password)
    {
        // Redshift uses PostgreSQL protocol
        return $"Host={config.Host};Port={config.Port};Database={config.Database};" +
               $"Username={config.Username};Password={password};" +
               $"Timeout={config.QueryTimeoutSeconds};Pooling=true;MinPoolSize=1;MaxPoolSize=20;" +
               $"SSL Mode=Require;";
    }

    private string DecryptPassword(string encryptedPassword)
    {
        // TODO: Implement proper encryption/decryption
        // For now, using simple Base64 encoding
        // In production, use AWS Secrets Manager, HashiCorp Vault, or proper AES encryption
        try
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedPassword));
        }
        catch
        {
            _logger.LogWarning("Failed to decrypt password, using as-is");
            return encryptedPassword;
        }
    }
}
