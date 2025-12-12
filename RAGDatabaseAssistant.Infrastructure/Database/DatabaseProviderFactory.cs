using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RAGDatabaseAssistant.Core.Interfaces;

namespace RAGDatabaseAssistant.Infrastructure.Database;

public class DatabaseProviderFactory : IDatabaseProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<string> _availableKeys;

    public DatabaseProviderFactory(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        
        // Obtener lista de nombres de bases de datos configuradas
        var databases = configuration.GetSection("Data:Databases")
            .Get<List<Databases>>() ?? new();
        
        _availableKeys = databases
            .Where(db => db.Enabled)
            .Select(db => db.Name)
            .ToList();
    }

    public IDatabaseProvider GetProvider(string name)
    {
        if (!_availableKeys.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException(
                $"Database '{name}' not found. Available: {string.Join(", ", _availableKeys)}");
        }
        
        // Resolver usando keyed service
        var provider = _serviceProvider.GetKeyedService<IDatabaseProvider>(name);
        
        if (provider == null)
        {
            throw new InvalidOperationException(
                $"Provider for database '{name}' could not be resolved.");
        }
        
        return provider;
    }
    
    public IEnumerable<string> GetAvailableDatabases()
    {
        return _availableKeys;
    }
}