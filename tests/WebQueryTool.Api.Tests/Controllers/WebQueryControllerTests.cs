using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebQueryTool.Api.Controllers;
using WebQueryTool.Api.Models;
using WebQueryTool.Api.Services;
using Xunit;

namespace WebQueryTool.Api.Tests.Controllers;

public class WebQueryControllerTests
{
    private readonly Mock<IWebQueryService> _mockWebQueryService;
    private readonly Mock<ILogger<WebQueryController>> _mockLogger;
    private readonly WebQueryController _controller;

    public WebQueryControllerTests()
    {
        _mockWebQueryService = new Mock<IWebQueryService>();
        _mockLogger = new Mock<ILogger<WebQueryController>>();
        _controller = new WebQueryController(_mockWebQueryService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task QueryWebPage_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new WebQueryRequest
        {
            Url = "https://example.com",
            QueryType = QueryType.Text
        };

        var expectedResponse = new WebQueryResponse
        {
            Success = true,
            Content = "Test content"
        };

        _mockWebQueryService
            .Setup(s => s.QueryWebPageAsync(It.IsAny<WebQueryRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.QueryWebPage(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task QueryWebPage_WithEmptyUrl_ReturnsBadRequest()
    {
        // Arrange
        var request = new WebQueryRequest
        {
            Url = "",
            QueryType = QueryType.Text
        };

        // Act
        var result = await _controller.QueryWebPage(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetHtml_WithValidUrl_ReturnsOkResult()
    {
        // Arrange
        var url = "https://example.com";
        var expectedResponse = new WebQueryResponse
        {
            Success = true,
            Content = "<html><body>Test</body></html>"
        };

        _mockWebQueryService
            .Setup(s => s.QueryWebPageAsync(It.Is<WebQueryRequest>(r =>
                r.Url == url && r.QueryType == QueryType.HtmlContent)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetHtml(url);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetText_WithValidUrl_ReturnsOkResult()
    {
        // Arrange
        var url = "https://example.com";
        var expectedResponse = new WebQueryResponse
        {
            Success = true,
            Content = "Test content"
        };

        _mockWebQueryService
            .Setup(s => s.QueryWebPageAsync(It.Is<WebQueryRequest>(r =>
                r.Url == url && r.QueryType == QueryType.Text)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetText(url);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLinks_WithValidUrl_ReturnsOkResult()
    {
        // Arrange
        var url = "https://example.com";
        var expectedResponse = new WebQueryResponse
        {
            Success = true,
            Items = new List<string> { "https://link1.com", "https://link2.com" }
        };

        _mockWebQueryService
            .Setup(s => s.QueryWebPageAsync(It.Is<WebQueryRequest>(r =>
                r.Url == url && r.QueryType == QueryType.Links)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetLinks(url);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetImages_WithValidUrl_ReturnsOkResult()
    {
        // Arrange
        var url = "https://example.com";
        var expectedResponse = new WebQueryResponse
        {
            Success = true,
            Items = new List<string> { "image1.jpg", "image2.png" }
        };

        _mockWebQueryService
            .Setup(s => s.QueryWebPageAsync(It.Is<WebQueryRequest>(r =>
                r.Url == url && r.QueryType == QueryType.Images)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetImages(url);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMetadata_WithValidUrl_ReturnsOkResult()
    {
        // Arrange
        var url = "https://example.com";
        var expectedResponse = new WebQueryResponse
        {
            Success = true,
            Metadata = new Dictionary<string, string>
            {
                { "title", "Example Page" },
                { "description", "Example description" }
            }
        };

        _mockWebQueryService
            .Setup(s => s.QueryWebPageAsync(It.Is<WebQueryRequest>(r =>
                r.Url == url && r.QueryType == QueryType.Metadata)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetMetadata(url);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Health_ReturnsOkResult()
    {
        // Act
        var result = _controller.Health();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
