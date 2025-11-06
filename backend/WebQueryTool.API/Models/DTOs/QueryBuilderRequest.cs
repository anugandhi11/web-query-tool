using System.ComponentModel.DataAnnotations;

namespace WebQueryTool.API.Models.DTOs;

/// <summary>
/// Request to build and execute a SQL query from parameters
/// WAF-FRIENDLY: No SQL keywords in request, built server-side
/// </summary>
public class QueryBuilderRequest
{
    /// <summary>
    /// Database connection ID
    /// </summary>
    [Required]
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// Table name to query
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Table { get; set; } = string.Empty;

    /// <summary>
    /// Columns to select (empty = SELECT *)
    /// </summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// Filters to apply (WHERE clause)
    /// </summary>
    public List<QueryFilter> Filters { get; set; } = new();

    /// <summary>
    /// Sort order
    /// </summary>
    public QueryOrderBy? OrderBy { get; set; }

    /// <summary>
    /// Maximum rows to return
    /// </summary>
    [Range(1, 10000)]
    public int Limit { get; set; } = 1000;

    /// <summary>
    /// Offset for pagination
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;
}

/// <summary>
/// A single filter condition
/// </summary>
public class QueryFilter
{
    [Required]
    [MaxLength(200)]
    public string Column { get; set; } = string.Empty;

    [Required]
    public FilterOperator Operator { get; set; }

    /// <summary>
    /// Filter value (will be parameterized - SQL injection safe)
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// Supported filter operators
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Like,
    NotLike,
    In,
    NotIn,
    IsNull,
    IsNotNull
}

/// <summary>
/// Sort order specification
/// </summary>
public class QueryOrderBy
{
    [Required]
    [MaxLength(200)]
    public string Column { get; set; } = string.Empty;

    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

public enum SortDirection
{
    Ascending,
    Descending
}
