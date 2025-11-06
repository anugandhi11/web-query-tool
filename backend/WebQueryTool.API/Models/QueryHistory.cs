using System.ComponentModel.DataAnnotations;

namespace WebQueryTool.API.Models;

/// <summary>
/// Audit log of executed queries
/// </summary>
public class QueryHistory
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// The actual SQL query executed (for audit purposes)
    /// </summary>
    [Required]
    public string SqlQuery { get; set; } = string.Empty;

    /// <summary>
    /// How the query was submitted (Encoded, Builder, Template)
    /// </summary>
    [Required]
    public QuerySubmissionType SubmissionType { get; set; }

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Number of rows returned
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if query failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Client IP address
    /// </summary>
    public string? IpAddress { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual DatabaseConnection Connection { get; set; } = null!;
}

public enum QuerySubmissionType
{
    EncodedQuery,      // Base64 encoded SQL
    QueryBuilder,      // Built from parameters
    Template,          // Predefined template
    Direct             // Direct SQL (should be disabled in production)
}
