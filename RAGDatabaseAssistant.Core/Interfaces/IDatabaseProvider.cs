using RAGDatabaseAssistant.Core.Models;

namespace RAGDatabaseAssistant.Core.Interfaces;

public interface IDatabaseProvider
{
    string Name { get; }
    string  ConnectionString { get; }
    DatabaseType Type { get; }
    
    // Conexión
    Task<bool> TestConnectionAsync();
    
    // Schema inspection
    Task<DatabaseSemanticSchema> GetSchemaAsync();
    Task<EntitySchema> GetTableSchemaAsync(string tableName );
    Task<List<string>> GetTablesAsync();
    
    // Query execution
    Task<QueryResult> ExecuteQueryAsync(string query);
    Task<string> ExplainQueryAsync(string query);
    
    // Metadata
    Task<QueryStatistics> GetQueryStatisticsAsync(string query);
    Task<List<IndexInfo>> GetIndexesAsync(string tableName);
    
    // Dialect-specific
    string TranslateToDialect(string genericQuery);
    string GetOptimalDataType(string genericType);
}

public enum DatabaseType
{
    PostgreSQL,
    MySQL,
    SQLServer,
    MongoDB,
    SQLite
}