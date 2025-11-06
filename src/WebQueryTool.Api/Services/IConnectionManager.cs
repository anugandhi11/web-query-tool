using WebQueryTool.Api.Models;

namespace WebQueryTool.Api.Services;

public interface IConnectionManager
{
    string AddConnection(DatabaseConnection connection);
    bool RemoveConnection(string connectionId);
    DatabaseConnection? GetConnection(string connectionId);
    List<DatabaseConnection> GetAllConnections();
    bool UpdateConnection(DatabaseConnection connection);
}
