using System.Text;

namespace WebQueryTool.API.Services;

/// <summary>
/// Implementation of query encoding/decoding service
/// WAF-FRIENDLY: Encodes SQL to Base64 so WAF cannot pattern match
/// </summary>
public class QueryEncodingService : IQueryEncodingService
{
    private readonly ILogger<QueryEncodingService> _logger;

    public QueryEncodingService(ILogger<QueryEncodingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Encode SQL query to Base64
    /// Example: "SELECT * FROM users" → "U0VMRUNUICogRlJPTSB1c2Vycw=="
    /// </summary>
    public string Encode(string sqlQuery)
    {
        if (string.IsNullOrEmpty(sqlQuery))
        {
            throw new ArgumentException("SQL query cannot be empty", nameof(sqlQuery));
        }

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(sqlQuery);
            string base64 = Convert.ToBase64String(bytes);

            _logger.LogDebug("Encoded SQL query of length {Length} to Base64", sqlQuery.Length);

            return base64;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encode SQL query");
            throw new InvalidOperationException("Failed to encode SQL query", ex);
        }
    }

    /// <summary>
    /// Decode Base64-encoded SQL query
    /// Example: "U0VMRUNUICogRlJPTSB1c2Vycw==" → "SELECT * FROM users"
    /// </summary>
    public string Decode(string encodedQuery)
    {
        if (string.IsNullOrEmpty(encodedQuery))
        {
            throw new ArgumentException("Encoded query cannot be empty", nameof(encodedQuery));
        }

        if (!IsValidBase64(encodedQuery))
        {
            throw new ArgumentException("Invalid Base64 string", nameof(encodedQuery));
        }

        try
        {
            byte[] bytes = Convert.FromBase64String(encodedQuery);
            string sqlQuery = Encoding.UTF8.GetString(bytes);

            _logger.LogDebug("Decoded Base64 string to SQL query of length {Length}", sqlQuery.Length);

            return sqlQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decode Base64 query");
            throw new InvalidOperationException("Failed to decode Base64 query", ex);
        }
    }

    /// <summary>
    /// Validate that a string is valid Base64
    /// </summary>
    public bool IsValidBase64(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Base64 string length must be multiple of 4
        if (value.Length % 4 != 0)
        {
            return false;
        }

        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
