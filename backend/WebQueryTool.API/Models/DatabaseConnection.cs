using System.ComponentModel.DataAnnotations;

namespace WebQueryTool.API.Models;

/// <summary>
/// Represents a database connection configuration
/// </summary>
public class DatabaseConnection
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DatabaseType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Host { get; set; } = string.Empty;

    [Required]
    public int Port { get; set; }

    [Required]
    [MaxLength(200)]
    public string Database { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted password - NEVER store plaintext!
    /// </summary>
    [Required]
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>
    /// Additional connection properties (JSON)
    /// </summary>
    public string? AdditionalProperties { get; set; }

    /// <summary>
    /// Owner user ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastTestedAt { get; set; }

    public bool LastTestSuccessful { get; set; }

    /// <summary>
    /// Maximum query timeout for this connection (seconds)
    /// </summary>
    public int QueryTimeoutSeconds { get; set; } = 60;
}

public enum DatabaseType
{
    PostgreSQL,
    MySQL,
    SQLServer,
    Redshift
}
