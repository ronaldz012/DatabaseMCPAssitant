namespace RAGDatabaseAssistant.Core.Interfaces;

public interface IDatabaseProviderFactory
{
    IDatabaseProvider GetProvider(string name);
}