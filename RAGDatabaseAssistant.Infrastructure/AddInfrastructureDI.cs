using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RAGDatabaseAssistant.Infrastructure.Database;
using RAGDatabaseAssistant.Infrastructure.Services;

namespace RAGDatabaseAssistant.Infrastructure;

public static class AddInfrastructureDI //Dependency Injection :D
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Data>(configuration.GetSection(Data.SectionName));

        IConfigurationSection configurationSection = configuration.GetSection("Embeddings");
        services.Configure<ServiceTest.Embeddings>(configurationSection);
        
        services.AddSingleton<DataBaseInfo>();
        services.AddSingleton<ServiceTest>();
        return services;
    }
}