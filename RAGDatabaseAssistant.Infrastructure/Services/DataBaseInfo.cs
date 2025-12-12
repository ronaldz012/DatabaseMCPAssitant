using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RAGDatabaseAssistant.Infrastructure.Database;
using RAGDatabaseAssistant.Infrastructure.Dtos;

namespace RAGDatabaseAssistant.Infrastructure.Services;

public class DataBaseInfo(IOptions<Data> data)
{
    private readonly List<Databases> _databases = data.Value.Databases;

    public async Task<List<DatabaseStatusDto>> GetDatabases()
    {
        
        var enabledDatabases = _databases
            .Where(db => db.Enabled)
            .ToList();
            
        Console.WriteLine($"{enabledDatabases.Count} database providers are enabled.");
        var statusDtos = enabledDatabases.Select(db => new DatabaseStatusDto
        {
            Name = db.Name,
            ProviderType = db.ProviderType,
            Enabled = db.Enabled,
        }).ToList();

        return await Task.FromResult(statusDtos);
    }
}