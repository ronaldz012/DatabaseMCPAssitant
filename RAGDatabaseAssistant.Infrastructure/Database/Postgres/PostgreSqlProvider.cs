using Microsoft.Extensions.Configuration;
using Npgsql;
using RAGDatabaseAssistant.Core.Interfaces;
using RAGDatabaseAssistant.Core.Models;
using RAGDatabaseAssistant.Infrastructure.Database.Postgres;
using DatabaseType = RAGDatabaseAssistant.Core.Interfaces.DatabaseType;

namespace RAGDatabaseAssistant.Infrastructure.Database;

public class PostgreSqlProvider(string name,string connectionString ) : IDatabaseProvider
{
    private NpgsqlConnection _connection;
    
    public string Name => name;
    public string ConnectionString => connectionString;
    public DatabaseType Type { get; } = DatabaseType.PostgreSQL;
    public async Task<bool> TestConnectionAsync()
    {
 
            await using var conn = new NpgsqlConnection();
            conn.ConnectionString = connectionString;
            await conn.OpenAsync();
            return true;
    }
    
    public async Task<DatabaseSemanticSchema> GetSchemaAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        
        var schema = new DatabaseSemanticSchema
        {
            DatabaseType = Core.Models.DatabaseType.PostgreSQL.ToString(),
            Entities = new List<EntitySchema>()
        };
        
        var tables = await GetTablesAsync();
        
        foreach (var tableName in tables)
        {
            var tableSchema = await GetTableSchemaAsync(tableName);
            schema.Entities.Add(tableSchema);
        }
        
        return schema;
    }

    public async Task<EntitySchema> GetTableSchemaAsync(string tableName)
    {
        var tableSchema = await Queries.GetTechnicalTableSchemaAsync(connectionString, tableName);
        return  SemanticMapper.MapEntity(tableSchema);
    }

    public async Task<List<string>> GetTablesAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        
        await using var cmd = new NpgsqlCommand(@"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public'
            ORDER BY table_name",
            conn);
        
        var tables = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        
        return tables;
    }

    public Task<QueryResult> ExecuteQueryAsync(string query)
    {
        throw new NotImplementedException();
    }

    public Task<string> ExplainQueryAsync(string query)
    {
        throw new NotImplementedException();
    }

    public Task<QueryStatistics> GetQueryStatisticsAsync(string query)
    {
        throw new NotImplementedException();
    }

    public Task<List<IndexInfo>> GetIndexesAsync(string tableName)
    {
        throw new NotImplementedException();
    }

    public string TranslateToDialect(string genericQuery)
    {
        throw new NotImplementedException();
    }

    public string GetOptimalDataType(string genericType)
    {
        throw new NotImplementedException();
    }
}