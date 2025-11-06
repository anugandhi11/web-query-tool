namespace WebQueryTool.API.Services;

/// <summary>
/// Service for encoding/decoding SQL queries to prevent WAF false positives
/// </summary>
public interface IQueryEncodingService
{
    /// <summary>
    /// Encode SQL query to Base64
    /// </summary>
    string Encode(string sqlQuery);

    /// <summary>
    /// Decode Base64-encoded SQL query
    /// </summary>
    string Decode(string encodedQuery);

    /// <summary>
    /// Validate that a string is valid Base64
    /// </summary>
    bool IsValidBase64(string value);
}
