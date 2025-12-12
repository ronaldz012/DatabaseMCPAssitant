namespace RAGDatabaseAssistant.Core.Models;

/// <summary>
/// Representa el schema completo de una base de datos
/// </summary>
public class DatabaseSchema
{
    public DatabaseType DatabaseType { get; set; }
    public List<TableSchema> Tables { get; set; } = new();
    public List<ViewSchema> Views { get; set; } = new();
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}

public enum DatabaseType
{
    PostgreSQL,
    MySQL,
    SQLServer,
    MongoDB,
    SQLite,
    Oracle
}