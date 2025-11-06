using WebQueryTool.Api.Models;

namespace WebQueryTool.Api.Services;

public interface IDatabaseService
{
    Task<TestConnectionResponse> TestConnectionAsync(TestConnectionRequest request);
    Task<ExecuteQueryResponse> ExecuteQueryAsync(DatabaseConnection connection, string query, int maxRows = 1000);
    Task<SchemaInfo> GetSchemaAsync(DatabaseConnection connection);
    Task<List<string>> GetDatabasesAsync(DatabaseConnection connection);
}
