using Microsoft.AspNetCore.Mvc;
using WebQueryTool.Api.Models;
using WebQueryTool.Api.Services;

namespace WebQueryTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebQueryController : ControllerBase
{
    private readonly IWebQueryService _webQueryService;
    private readonly ILogger<WebQueryController> _logger;

    public WebQueryController(IWebQueryService webQueryService, ILogger<WebQueryController> logger)
    {
        _webQueryService = webQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Query a web page and extract content based on the specified parameters
    /// </summary>
    /// <param name="request">The web query request containing URL, selector, and query type</param>
    /// <returns>The query result</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(WebQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebQueryResponse>> QueryWebPage([FromBody] WebQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new WebQueryResponse
            {
                Success = false,
                ErrorMessage = "URL is required"
            });
        }

        _logger.LogInformation("Querying web page: {Url} with query type: {QueryType}", request.Url, request.QueryType);

        var result = await _webQueryService.QueryWebPageAsync(request);

        if (!result.Success)
        {
            _logger.LogWarning("Query failed for URL: {Url}. Error: {Error}", request.Url, result.ErrorMessage);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get the HTML content of a web page
    /// </summary>
    /// <param name="url">The URL of the web page</param>
    /// <param name="selector">Optional CSS selector to filter content</param>
    /// <returns>The HTML content</returns>
    [HttpGet("html")]
    [ProducesResponseType(typeof(WebQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebQueryResponse>> GetHtml([FromQuery] string url, [FromQuery] string? selector = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new WebQueryResponse
            {
                Success = false,
                ErrorMessage = "URL is required"
            });
        }

        var request = new WebQueryRequest
        {
            Url = url,
            Selector = selector,
            QueryType = QueryType.HtmlContent
        };

        var result = await _webQueryService.QueryWebPageAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get the text content of a web page
    /// </summary>
    /// <param name="url">The URL of the web page</param>
    /// <param name="selector">Optional CSS selector to filter content</param>
    /// <returns>The text content</returns>
    [HttpGet("text")]
    [ProducesResponseType(typeof(WebQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebQueryResponse>> GetText([FromQuery] string url, [FromQuery] string? selector = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new WebQueryResponse
            {
                Success = false,
                ErrorMessage = "URL is required"
            });
        }

        var request = new WebQueryRequest
        {
            Url = url,
            Selector = selector,
            QueryType = QueryType.Text
        };

        var result = await _webQueryService.QueryWebPageAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get all links from a web page
    /// </summary>
    /// <param name="url">The URL of the web page</param>
    /// <param name="selector">Optional CSS selector to filter links</param>
    /// <returns>List of links</returns>
    [HttpGet("links")]
    [ProducesResponseType(typeof(WebQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebQueryResponse>> GetLinks([FromQuery] string url, [FromQuery] string? selector = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new WebQueryResponse
            {
                Success = false,
                ErrorMessage = "URL is required"
            });
        }

        var request = new WebQueryRequest
        {
            Url = url,
            Selector = selector,
            QueryType = QueryType.Links
        };

        var result = await _webQueryService.QueryWebPageAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get all images from a web page
    /// </summary>
    /// <param name="url">The URL of the web page</param>
    /// <param name="selector">Optional CSS selector to filter images</param>
    /// <returns>List of image URLs</returns>
    [HttpGet("images")]
    [ProducesResponseType(typeof(WebQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebQueryResponse>> GetImages([FromQuery] string url, [FromQuery] string? selector = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new WebQueryResponse
            {
                Success = false,
                ErrorMessage = "URL is required"
            });
        }

        var request = new WebQueryRequest
        {
            Url = url,
            Selector = selector,
            QueryType = QueryType.Images
        };

        var result = await _webQueryService.QueryWebPageAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get metadata from a web page (title, meta tags, etc.)
    /// </summary>
    /// <param name="url">The URL of the web page</param>
    /// <returns>Page metadata</returns>
    [HttpGet("metadata")]
    [ProducesResponseType(typeof(WebQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebQueryResponse>> GetMetadata([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new WebQueryResponse
            {
                Success = false,
                ErrorMessage = "URL is required"
            });
        }

        var request = new WebQueryRequest
        {
            Url = url,
            QueryType = QueryType.Metadata
        };

        var result = await _webQueryService.QueryWebPageAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>API status</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
