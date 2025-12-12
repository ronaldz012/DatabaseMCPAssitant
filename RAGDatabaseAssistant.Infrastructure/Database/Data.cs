namespace RAGDatabaseAssistant.Infrastructure.Database;


public class Data
{
    public const string SectionName = "Data";

    public List<Databases> Databases { get; set; } = new();
}

public class Databases
{
    public required string Name { get; set; }

    public string Description { get; set; } = string.Empty;
    public required string ProviderType { get; set; }
    public bool Enabled { get; set; } = false;
    public required string ConnectionString { get; set; }
}
