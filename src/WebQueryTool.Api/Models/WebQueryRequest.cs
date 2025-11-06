namespace WebQueryTool.Api.Models;

public class WebQueryRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Selector { get; set; }
    public QueryType QueryType { get; set; } = QueryType.HtmlContent;
}

public enum QueryType
{
    HtmlContent,
    Text,
    Links,
    Images,
    Metadata
}
