namespace RAGDatabaseAssistant.Core.Models;

public class DatabaseSemanticSchema
{
    public string DatabaseType { get; set; } = "PostgreSQL";
    public List<EntitySchema> Entities { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}