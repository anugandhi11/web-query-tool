using WebQueryTool.Api.Models;

namespace WebQueryTool.Api.Services;

public interface IWebQueryService
{
    Task<WebQueryResponse> QueryWebPageAsync(WebQueryRequest request);
}
