using AngleSharp;
using AngleSharp.Html.Parser;
using WebQueryTool.Api.Models;

namespace WebQueryTool.Api.Services;

public class WebQueryService : IWebQueryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebQueryService> _logger;

    public WebQueryService(IHttpClientFactory httpClientFactory, ILogger<WebQueryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<WebQueryResponse> QueryWebPageAsync(WebQueryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return new WebQueryResponse
                {
                    Success = false,
                    ErrorMessage = "URL is required"
                };
            }

            // Validate URL
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
            {
                return new WebQueryResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid URL format"
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var response = await httpClient.GetAsync(request.Url);

            if (!response.IsSuccessStatusCode)
            {
                return new WebQueryResponse
                {
                    Success = false,
                    ErrorMessage = $"HTTP Error: {response.StatusCode}"
                };
            }

            var html = await response.Content.ReadAsStringAsync();

            // Parse HTML using AngleSharp
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(html);

            WebQueryResponse result = request.QueryType switch
            {
                QueryType.HtmlContent => await GetHtmlContent(document, request.Selector),
                QueryType.Text => await GetTextContent(document, request.Selector),
                QueryType.Links => await GetLinks(document, request.Selector),
                QueryType.Images => await GetImages(document, request.Selector),
                QueryType.Metadata => await GetMetadata(document),
                _ => new WebQueryResponse { Success = false, ErrorMessage = "Invalid query type" }
            };

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for URL: {Url}", request.Url);
            return new WebQueryResponse
            {
                Success = false,
                ErrorMessage = $"Request failed: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying web page: {Url}", request.Url);
            return new WebQueryResponse
            {
                Success = false,
                ErrorMessage = $"An error occurred: {ex.Message}"
            };
        }
    }

    private async Task<WebQueryResponse> GetHtmlContent(AngleSharp.Dom.IDocument document, string? selector)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(selector))
        {
            return new WebQueryResponse
            {
                Success = true,
                Content = document.DocumentElement.OuterHtml
            };
        }

        var elements = document.QuerySelectorAll(selector);
        var htmlContents = elements.Select(e => e.OuterHtml).ToList();

        return new WebQueryResponse
        {
            Success = true,
            Items = htmlContents
        };
    }

    private async Task<WebQueryResponse> GetTextContent(AngleSharp.Dom.IDocument document, string? selector)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(selector))
        {
            return new WebQueryResponse
            {
                Success = true,
                Content = document.Body?.TextContent?.Trim()
            };
        }

        var elements = document.QuerySelectorAll(selector);
        var textContents = elements.Select(e => e.TextContent.Trim()).ToList();

        return new WebQueryResponse
        {
            Success = true,
            Items = textContents
        };
    }

    private async Task<WebQueryResponse> GetLinks(AngleSharp.Dom.IDocument document, string? selector)
    {
        await Task.CompletedTask;

        var query = string.IsNullOrWhiteSpace(selector) ? "a[href]" : selector;
        var links = document.QuerySelectorAll(query)
            .Select(e => e.GetAttribute("href"))
            .Where(href => !string.IsNullOrWhiteSpace(href))
            .Select(href => href!)
            .ToList();

        return new WebQueryResponse
        {
            Success = true,
            Items = links
        };
    }

    private async Task<WebQueryResponse> GetImages(AngleSharp.Dom.IDocument document, string? selector)
    {
        await Task.CompletedTask;

        var query = string.IsNullOrWhiteSpace(selector) ? "img[src]" : selector;
        var images = document.QuerySelectorAll(query)
            .Select(e => e.GetAttribute("src"))
            .Where(src => !string.IsNullOrWhiteSpace(src))
            .Select(src => src!)
            .ToList();

        return new WebQueryResponse
        {
            Success = true,
            Items = images
        };
    }

    private async Task<WebQueryResponse> GetMetadata(AngleSharp.Dom.IDocument document)
    {
        await Task.CompletedTask;

        var metadata = new Dictionary<string, string>();

        // Get title
        var title = document.QuerySelector("title")?.TextContent;
        if (!string.IsNullOrWhiteSpace(title))
        {
            metadata["title"] = title.Trim();
        }

        // Get meta tags
        var metaTags = document.QuerySelectorAll("meta");
        foreach (var meta in metaTags)
        {
            var name = meta.GetAttribute("name") ?? meta.GetAttribute("property");
            var content = meta.GetAttribute("content");

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(content))
            {
                metadata[name] = content;
            }
        }

        return new WebQueryResponse
        {
            Success = true,
            Metadata = metadata
        };
    }
}
