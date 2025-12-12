using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace RAGDatabaseAssistant.Infrastructure.Services;

public class ServiceTest(IConfiguration configuration, IOptions<ServiceTest.Embeddings> options)
{
    private readonly Embeddings embeddings = options.Value;
    public async Task<string> testTheServices()
    {
        return await Task.FromResult("This is a test, all good");
    }

    public async Task<string> testIConfig()
    {
        var lecture = configuration.GetSection("Embeddings:Provider").Value;
        if (lecture is null)
        {
            return await Task.FromResult("could not find config file or config");
        }
        return await Task.FromResult(lecture);
    }
    
    public async Task<Embeddings> testIOptions()
    {
        return await Task.FromResult<Embeddings>(embeddings);
    }
    
    public class Embeddings
    {
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }
    
    // "Embeddings": {
    //     "Provider": "Ollama",
    //     "Model": "nomic-embed-text",
    //     "Endpoint": "http://localhost:11434"
    // },
}