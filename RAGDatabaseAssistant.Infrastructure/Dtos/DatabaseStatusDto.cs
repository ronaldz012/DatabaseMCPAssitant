namespace RAGDatabaseAssistant.Infrastructure.Dtos;

public class DatabaseStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}