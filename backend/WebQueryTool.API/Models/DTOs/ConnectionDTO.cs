using System.ComponentModel.DataAnnotations;

namespace WebQueryTool.API.Models.DTOs;

/// <summary>
/// Request to create a new database connection
/// </summary>
public class CreateConnectionRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DatabaseType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Host { get; set; } = string.Empty;

    [Required]
    [Range(1, 65535)]
    public int Port { get; set; }

    [Required]
    [MaxLength(200)]
    public string Database { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Range(10, 300)]
    public int QueryTimeoutSeconds { get; set; } = 60;
}

/// <summary>
/// Response containing connection information (without password)
/// </summary>
public class ConnectionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DatabaseType Type { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public bool LastTestSuccessful { get; set; }
    public int QueryTimeoutSeconds { get; set; }
}

/// <summary>
/// Result of testing a connection
/// </summary>
public class ConnectionTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public string? ServerVersion { get; set; }
}
