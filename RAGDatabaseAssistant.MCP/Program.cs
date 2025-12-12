using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RAGDatabaseAssistant.Core.Interfaces;
using RAGDatabaseAssistant.Infrastructure;
using RAGDatabaseAssistant.Infrastructure.Database;
using RAGDatabaseAssistant.MCP.Tools;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var providerOptions = config.GetSection("Data:Databases")
    .Get<List<Databases>>() ?? new List<Databases>();

foreach (var option in providerOptions.Where(o => o.Enabled))
{
    Console.Error.WriteLine($"Registering database {option.Name}");
    builder.Services.AddKeyedSingleton<IDatabaseProvider>(
        option.Name,  
        (sp, key) =>
        {
            return option.ProviderType.ToLowerInvariant() switch
            {
                "postgresql" or "postgres" => new PostgreSqlProvider(
                    option.Name,
                    option.ConnectionString
                ),
                _ => throw new InvalidOperationException(
                    $"Unknown provider: {option.ProviderType}")
            };
        });
}
builder.Services.AddSingleton<IDatabaseProviderFactory, DatabaseProviderFactory>();

builder.Services.AddSingleton<IConfiguration>(config);


builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    //.WithTools<Test>()
    .WithTools<QueryTool>();
builder.Services.AddInfrastructure(config);

// Registrar QueryTool (DESPUÉS de MCP)
builder.Services.AddSingleton<QueryTool>();
await builder.Build().RunAsync();