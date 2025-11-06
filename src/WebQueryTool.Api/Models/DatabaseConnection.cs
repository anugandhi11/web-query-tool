namespace WebQueryTool.Api.Models;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSQL,
    MySQL
}

public class DatabaseConnection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DatabaseProvider Provider { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TestConnectionRequest
{
    public DatabaseProvider Provider { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = false;
}

public class TestConnectionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ServerVersion { get; set; }
}
