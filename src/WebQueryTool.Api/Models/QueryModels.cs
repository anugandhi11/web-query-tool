namespace WebQueryTool.Api.Models;

public class ExecuteQueryRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int MaxRows { get; set; } = 1000;
}

public class ExecuteQueryResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? ColumnNames { get; set; }
    public List<Dictionary<string, object?>>? Rows { get; set; }
    public int RowCount { get; set; }
    public long ExecutionTimeMs { get; set; }
}

public class SchemaInfo
{
    public List<TableInfo>? Tables { get; set; }
    public List<ViewInfo>? Views { get; set; }
}

public class TableInfo
{
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = string.Empty;
    public List<ColumnInfo>? Columns { get; set; }
}

public class ViewInfo
{
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = string.Empty;
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
}
