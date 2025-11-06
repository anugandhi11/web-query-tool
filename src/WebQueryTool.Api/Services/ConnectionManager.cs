using System.Collections.Concurrent;
using WebQueryTool.Api.Models;

namespace WebQueryTool.Api.Services;

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, DatabaseConnection> _connections = new();
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger;
    }

    public string AddConnection(DatabaseConnection connection)
    {
        if (string.IsNullOrEmpty(connection.Id))
        {
            connection.Id = Guid.NewGuid().ToString();
        }

        connection.CreatedAt = DateTime.UtcNow;

        if (_connections.TryAdd(connection.Id, connection))
        {
            _logger.LogInformation("Added connection: {ConnectionId} - {ConnectionName}", connection.Id, connection.Name);
            return connection.Id;
        }

        throw new InvalidOperationException("Failed to add connection");
    }

    public bool RemoveConnection(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            _logger.LogInformation("Removed connection: {ConnectionId} - {ConnectionName}", connectionId, connection.Name);
            return true;
        }

        return false;
    }

    public DatabaseConnection? GetConnection(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var connection);
        return connection;
    }

    public List<DatabaseConnection> GetAllConnections()
    {
        return _connections.Values.ToList();
    }

    public bool UpdateConnection(DatabaseConnection connection)
    {
        if (!_connections.ContainsKey(connection.Id))
        {
            return false;
        }

        _connections[connection.Id] = connection;
        _logger.LogInformation("Updated connection: {ConnectionId} - {ConnectionName}", connection.Id, connection.Name);
        return true;
    }
}
