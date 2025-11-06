using System.ComponentModel.DataAnnotations;

namespace WebQueryTool.API.Models.DTOs;

/// <summary>
/// Request to execute a Base64-encoded SQL query
/// WAF-FRIENDLY: SQL is encoded, WAF cannot pattern match
/// </summary>
public class QueryExecuteRequest
{
    /// <summary>
    /// Base64-encoded SQL query
    /// Example: "SELECT * FROM users" becomes "U0VMRUNUICogRlJPTSB1c2Vycw=="
    /// </summary>
    [Required]
    public string QueryData { get; set; } = string.Empty;

    /// <summary>
    /// Database connection ID
    /// </summary>
    [Required]
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// Optional execution options
    /// </summary>
    public QueryExecutionOptions? Options { get; set; }
}

/// <summary>
/// Options for query execution
/// </summary>
public class QueryExecutionOptions
{
    /// <summary>
    /// Maximum number of rows to return
    /// </summary>
    public int MaxRows { get; set; } = 1000;

    /// <summary>
    /// Query timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Whether to include column metadata
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;
}

/// <summary>
/// Result of a query execution
/// </summary>
public class QueryResult
{
    public bool Success { get; set; }

    /// <summary>
    /// Column names in order
    /// </summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// Column metadata (type, nullable, etc.)
    /// </summary>
    public List<ColumnMetadata>? ColumnMetadata { get; set; }

    /// <summary>
    /// Rows as arrays of objects
    /// </summary>
    public List<List<object?>> Rows { get; set; } = new();

    /// <summary>
    /// Total number of rows returned
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Error message if query failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether more rows are available (result set was truncated)
    /// </summary>
    public bool HasMoreRows { get; set; }
}

/// <summary>
/// Metadata about a result column
/// </summary>
public class ColumnMetadata
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
}
