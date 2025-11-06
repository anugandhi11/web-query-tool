using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using WebQueryTool.Api.Models;
using WebQueryTool.Api.Services;
using Xunit;

namespace WebQueryTool.Api.Tests.Services;

public class WebQueryServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<WebQueryService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly WebQueryService _service;

    public WebQueryServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<WebQueryService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _service = new WebQueryService(_mockHttpClientFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task QueryWebPageAsync_WithEmptyUrl_ReturnsError()
    {
        // Arrange
        var request = new WebQueryRequest
        {
            Url = "",
            QueryType = QueryType.Text
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("URL is required");
    }

    [Fact]
    public async Task QueryWebPageAsync_WithInvalidUrl_ReturnsError()
    {
        // Arrange
        var request = new WebQueryRequest
        {
            Url = "not-a-valid-url",
            QueryType = QueryType.Text
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid URL format");
    }

    [Fact]
    public async Task QueryWebPageAsync_WithValidUrl_HtmlContent_ReturnsSuccess()
    {
        // Arrange
        var htmlContent = "<html><body><h1>Test</h1></body></html>";
        SetupHttpResponse(HttpStatusCode.OK, htmlContent);

        var request = new WebQueryRequest
        {
            Url = "https://example.com",
            QueryType = QueryType.HtmlContent
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task QueryWebPageAsync_WithValidUrl_Text_ReturnsSuccess()
    {
        // Arrange
        var htmlContent = "<html><body><h1>Test Heading</h1><p>Test paragraph</p></body></html>";
        SetupHttpResponse(HttpStatusCode.OK, htmlContent);

        var request = new WebQueryRequest
        {
            Url = "https://example.com",
            QueryType = QueryType.Text
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task QueryWebPageAsync_WithValidUrl_Links_ReturnsSuccess()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <a href='https://link1.com'>Link 1</a>
                    <a href='https://link2.com'>Link 2</a>
                </body>
            </html>";
        SetupHttpResponse(HttpStatusCode.OK, htmlContent);

        var request = new WebQueryRequest
        {
            Url = "https://example.com",
            QueryType = QueryType.Links
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryWebPageAsync_WithValidUrl_Images_ReturnsSuccess()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <img src='image1.jpg' />
                    <img src='image2.png' />
                </body>
            </html>";
        SetupHttpResponse(HttpStatusCode.OK, htmlContent);

        var request = new WebQueryRequest
        {
            Url = "https://example.com",
            QueryType = QueryType.Images
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryWebPageAsync_WithValidUrl_Metadata_ReturnsSuccess()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <head>
                    <title>Test Page</title>
                    <meta name='description' content='Test description' />
                    <meta property='og:title' content='OG Title' />
                </head>
                <body></body>
            </html>";
        SetupHttpResponse(HttpStatusCode.OK, htmlContent);

        var request = new WebQueryRequest
        {
            Url = "https://example.com",
            QueryType = QueryType.Metadata
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().ContainKey("title");
        result.Metadata!["title"].Should().Be("Test Page");
    }

    [Fact]
    public async Task QueryWebPageAsync_WithHttpError_ReturnsError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "");

        var request = new WebQueryRequest
        {
            Url = "https://example.com",
            QueryType = QueryType.Text
        };

        // Act
        var result = await _service.QueryWebPageAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("HTTP Error");
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}
