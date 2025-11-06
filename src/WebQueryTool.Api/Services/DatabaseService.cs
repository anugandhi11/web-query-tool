using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using WebQueryTool.Api.Models;

namespace WebQueryTool.Api.Services;

public class DatabaseService : IDatabaseService
{
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(ILogger<DatabaseService> logger)
    {
        _logger = logger;
    }

    public async Task<TestConnectionResponse> TestConnectionAsync(TestConnectionRequest request)
    {
        try
        {
            var connectionString = BuildConnectionString(request);
            using var connection = CreateConnection(request.Provider, connectionString);

            await connection.OpenAsync();

            var version = connection.ServerVersion;

            return new TestConnectionResponse
            {
                Success = true,
                Message = "Connection successful",
                ServerVersion = version
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed");
            return new TestConnectionResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ExecuteQueryResponse> ExecuteQueryAsync(
        DatabaseConnection connection,
        string query,
        int maxRows = 1000)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var connectionString = BuildConnectionString(connection);
            using var dbConnection = CreateConnection(connection.Provider, connectionString);

            await dbConnection.OpenAsync();

            // Determine if it's a SELECT query or modification query
            var trimmedQuery = query.Trim();
            var isSelect = trimmedQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                          trimmedQuery.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
                          trimmedQuery.StartsWith("SHOW", StringComparison.OrdinalIgnoreCase);

            if (isSelect)
            {
                // Execute SELECT query
                using var reader = await dbConnection.ExecuteReaderAsync(query);
                var rows = new List<Dictionary<string, object?>>();
                var columnNames = new List<string>();

                // Get column names
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columnNames.Add(reader.GetName(i));
                }

                // Read rows (limited by maxRows)
                int rowCount = 0;
                while (await reader.ReadAsync() && rowCount < maxRows)
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        row[columnNames[i]] = value == DBNull.Value ? null : value;
                    }
                    rows.Add(row);
                    rowCount++;
                }

                stopwatch.Stop();

                return new ExecuteQueryResponse
                {
                    Success = true,
                    ColumnNames = columnNames,
                    Rows = rows,
                    RowCount = rowCount,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            else
            {
                // Execute INSERT, UPDATE, DELETE, CREATE, etc.
                var affectedRows = await dbConnection.ExecuteAsync(query);

                stopwatch.Stop();

                return new ExecuteQueryResponse
                {
                    Success = true,
                    RowCount = affectedRows,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = $"{affectedRows} row(s) affected"
                };
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query execution failed");

            return new ExecuteQueryResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<SchemaInfo> GetSchemaAsync(DatabaseConnection connection)
    {
        try
        {
            var connectionString = BuildConnectionString(connection);
            using var dbConnection = CreateConnection(connection.Provider, connectionString);

            await dbConnection.OpenAsync();

            var tables = await GetTablesAsync(dbConnection, connection.Provider);
            var views = await GetViewsAsync(dbConnection, connection.Provider);

            return new SchemaInfo
            {
                Tables = tables,
                Views = views
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get schema");
            throw;
        }
    }

    public async Task<List<string>> GetDatabasesAsync(DatabaseConnection connection)
    {
        try
        {
            var connectionString = BuildConnectionString(connection, useMasterDb: true);
            using var dbConnection = CreateConnection(connection.Provider, connectionString);

            await dbConnection.OpenAsync();

            string query = connection.Provider switch
            {
                DatabaseProvider.SqlServer => "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name",
                DatabaseProvider.PostgreSQL => "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname",
                DatabaseProvider.MySQL => "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA ORDER BY SCHEMA_NAME",
                _ => throw new NotSupportedException($"Provider {connection.Provider} not supported")
            };

            var databases = await dbConnection.QueryAsync<string>(query);
            return databases.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get databases");
            throw;
        }
    }

    private async Task<List<TableInfo>> GetTablesAsync(DbConnection connection, DatabaseProvider provider)
    {
        string query = provider switch
        {
            DatabaseProvider.SqlServer => @"
                SELECT
                    t.TABLE_SCHEMA as [Schema],
                    t.TABLE_NAME as [Name]
                FROM INFORMATION_SCHEMA.TABLES t
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME",

            DatabaseProvider.PostgreSQL => @"
                SELECT
                    table_schema as Schema,
                    table_name as Name
                FROM information_schema.tables
                WHERE table_type = 'BASE TABLE'
                AND table_schema NOT IN ('pg_catalog', 'information_schema')
                ORDER BY table_schema, table_name",

            DatabaseProvider.MySQL => @"
                SELECT
                    TABLE_SCHEMA as `Schema`,
                    TABLE_NAME as `Name`
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                AND TABLE_SCHEMA = DATABASE()
                ORDER BY TABLE_SCHEMA, TABLE_NAME",

            _ => throw new NotSupportedException($"Provider {provider} not supported")
        };

        var tables = await connection.QueryAsync<TableInfo>(query);
        return tables.ToList();
    }

    private async Task<List<ViewInfo>> GetViewsAsync(DbConnection connection, DatabaseProvider provider)
    {
        string query = provider switch
        {
            DatabaseProvider.SqlServer => @"
                SELECT
                    TABLE_SCHEMA as [Schema],
                    TABLE_NAME as [Name]
                FROM INFORMATION_SCHEMA.VIEWS
                ORDER BY TABLE_SCHEMA, TABLE_NAME",

            DatabaseProvider.PostgreSQL => @"
                SELECT
                    table_schema as Schema,
                    table_name as Name
                FROM information_schema.views
                WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
                ORDER BY table_schema, table_name",

            DatabaseProvider.MySQL => @"
                SELECT
                    TABLE_SCHEMA as `Schema`,
                    TABLE_NAME as `Name`
                FROM INFORMATION_SCHEMA.VIEWS
                WHERE TABLE_SCHEMA = DATABASE()
                ORDER BY TABLE_SCHEMA, TABLE_NAME",

            _ => throw new NotSupportedException($"Provider {provider} not supported")
        };

        var views = await connection.QueryAsync<ViewInfo>(query);
        return views.ToList();
    }

    private DbConnection CreateConnection(DatabaseProvider provider, string connectionString)
    {
        return provider switch
        {
            DatabaseProvider.SqlServer => new SqlConnection(connectionString),
            DatabaseProvider.PostgreSQL => new NpgsqlConnection(connectionString),
            DatabaseProvider.MySQL => new MySqlConnection(connectionString),
            _ => throw new NotSupportedException($"Provider {provider} not supported")
        };
    }

    private string BuildConnectionString(TestConnectionRequest request)
    {
        return request.Provider switch
        {
            DatabaseProvider.SqlServer =>
                $"Server={request.Host},{request.Port};Database={request.Database};User Id={request.Username};Password={request.Password};TrustServerCertificate=true;",

            DatabaseProvider.PostgreSQL =>
                $"Host={request.Host};Port={request.Port};Database={request.Database};Username={request.Username};Password={request.Password};SSL Mode={(request.UseSSL ? "Require" : "Disable")};",

            DatabaseProvider.MySQL =>
                $"Server={request.Host};Port={request.Port};Database={request.Database};Uid={request.Username};Pwd={request.Password};SslMode={(request.UseSSL ? "Required" : "None")};",

            _ => throw new NotSupportedException($"Provider {request.Provider} not supported")
        };
    }

    private string BuildConnectionString(DatabaseConnection connection, bool useMasterDb = false)
    {
        var database = useMasterDb ? "master" : connection.Database;

        return connection.Provider switch
        {
            DatabaseProvider.SqlServer =>
                $"Server={connection.Host},{connection.Port};Database={database};User Id={connection.Username};Password={connection.Password};TrustServerCertificate=true;",

            DatabaseProvider.PostgreSQL =>
                $"Host={connection.Host};Port={connection.Port};Database={database};Username={connection.Username};Password={connection.Password};SSL Mode={(connection.UseSSL ? "Require" : "Disable")};",

            DatabaseProvider.MySQL =>
                $"Server={connection.Host};Port={connection.Port};Database={database};Uid={connection.Username};Pwd={connection.Password};SslMode={(connection.UseSSL ? "Required" : "None")};",

            _ => throw new NotSupportedException($"Provider {connection.Provider} not supported")
        };
    }
}
