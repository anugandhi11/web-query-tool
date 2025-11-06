namespace WebQueryTool.Api.Models;

public class WebQueryResponse
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public List<string>? Items { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
