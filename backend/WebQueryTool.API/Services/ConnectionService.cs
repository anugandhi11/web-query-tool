using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebQueryTool.API.Data;
using WebQueryTool.API.Models;
using WebQueryTool.API.Models.DTOs;

namespace WebQueryTool.API.Services;

/// <summary>
/// Implementation of connection service
/// </summary>
public class ConnectionService : IConnectionService
{
    private readonly ApplicationDbContext _context;
    private readonly IConnectionPoolService _connectionPool;
    private readonly ILogger<ConnectionService> _logger;

    public ConnectionService(
        ApplicationDbContext context,
        IConnectionPoolService connectionPool,
        ILogger<ConnectionService> logger)
    {
        _context = context;
        _connectionPool = connectionPool;
        _logger = logger;
    }

    public async Task<DatabaseConnection> CreateConnectionAsync(CreateConnectionRequest request, string userId)
    {
        // Check if connection with same name exists for this user
        var existingConnection = await _context.DatabaseConnections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == request.Name);

        if (existingConnection != null)
        {
            throw new InvalidOperationException($"Connection with name '{request.Name}' already exists");
        }

        // Encrypt password
        var encryptedPassword = EncryptPassword(request.Password);

        // Create connection
        var connection = new DatabaseConnection
        {
            Name = request.Name,
            Type = request.Type,
            Host = request.Host,
            Port = request.Port,
            Database = request.Database,
            Username = request.Username,
            EncryptedPassword = encryptedPassword,
            UserId = userId,
            QueryTimeoutSeconds = request.QueryTimeoutSeconds,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.DatabaseConnections.Add(connection);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created connection {Name} for user {UserId}", connection.Name, userId);

        return connection;
    }

    public async Task<List<ConnectionResponse>> GetConnectionsAsync(string userId)
    {
        var connections = await _context.DatabaseConnections
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return connections.Select(MapToResponse).ToList();
    }

    public async Task<ConnectionResponse?> GetConnectionAsync(string connectionId, string userId)
    {
        var connection = await _context.DatabaseConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId && c.IsActive);

        return connection != null ? MapToResponse(connection) : null;
    }

    public async Task<DatabaseConnection> UpdateConnectionAsync(
        string connectionId,
        CreateConnectionRequest request,
        string userId)
    {
        var connection = await _context.DatabaseConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId);

        if (connection == null)
        {
            throw new NotFoundException($"Connection with ID '{connectionId}' not found");
        }

        // Update properties
        connection.Name = request.Name;
        connection.Type = request.Type;
        connection.Host = request.Host;
        connection.Port = request.Port;
        connection.Database = request.Database;
        connection.Username = request.Username;
        connection.QueryTimeoutSeconds = request.QueryTimeoutSeconds;

        // Only update password if provided
        if (!string.IsNullOrEmpty(request.Password))
        {
            connection.EncryptedPassword = EncryptPassword(request.Password);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated connection {Name} for user {UserId}", connection.Name, userId);

        return connection;
    }

    public async Task DeleteConnectionAsync(string connectionId, string userId)
    {
        var connection = await _context.DatabaseConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId);

        if (connection == null)
        {
            throw new NotFoundException($"Connection with ID '{connectionId}' not found");
        }

        // Soft delete
        connection.IsActive = false;
        await _context.SaveChangesAsync();

        // Close any open connections
        await _connectionPool.CloseConnectionsAsync(connectionId);

        _logger.LogInformation("Deleted connection {Name} for user {UserId}", connection.Name, userId);
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(string connectionId, string userId)
    {
        var connection = await _context.DatabaseConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId && c.IsActive);

        if (connection == null)
        {
            return new ConnectionTestResult
            {
                Success = false,
                Message = "Connection not found"
            };
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var success = await _connectionPool.TestConnectionAsync(connection);
            stopwatch.Stop();

            // Update last tested timestamp
            connection.LastTestedAt = DateTime.UtcNow;
            connection.LastTestSuccessful = success;
            await _context.SaveChangesAsync();

            return new ConnectionTestResult
            {
                Success = success,
                Message = success ? "Connection successful" : "Connection failed",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Connection test failed for {Name}", connection.Name);

            connection.LastTestedAt = DateTime.UtcNow;
            connection.LastTestSuccessful = false;
            await _context.SaveChangesAsync();

            return new ConnectionTestResult
            {
                Success = false,
                Message = $"Connection failed: {ex.Message}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<ConnectionTestResult> TestNewConnectionAsync(CreateConnectionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Create temporary connection object
            var tempConnection = new DatabaseConnection
            {
                Type = request.Type,
                Host = request.Host,
                Port = request.Port,
                Database = request.Database,
                Username = request.Username,
                EncryptedPassword = EncryptPassword(request.Password),
                QueryTimeoutSeconds = request.QueryTimeoutSeconds
            };

            var success = await _connectionPool.TestConnectionAsync(tempConnection);
            stopwatch.Stop();

            return new ConnectionTestResult
            {
                Success = success,
                Message = success ? "Connection successful" : "Connection failed",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Connection test failed for new connection");

            return new ConnectionTestResult
            {
                Success = false,
                Message = $"Connection failed: {ex.Message}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private ConnectionResponse MapToResponse(DatabaseConnection connection)
    {
        return new ConnectionResponse
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
            LastTestedAt = connection.LastTestedAt,
            LastTestSuccessful = connection.LastTestSuccessful,
            QueryTimeoutSeconds = connection.QueryTimeoutSeconds
        };
    }

    private string EncryptPassword(string password)
    {
        // TODO: Implement proper encryption
        // For now, using simple Base64 encoding
        // In production, use AWS Secrets Manager, HashiCorp Vault, or proper AES encryption
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
