using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using RAGDatabaseAssistant.Core.Interfaces;
using RAGDatabaseAssistant.Core.Models;
using RAGDatabaseAssistant.Infrastructure.Database;
using RAGDatabaseAssistant.Infrastructure.Dtos;
using RAGDatabaseAssistant.Infrastructure.Services;

namespace RAGDatabaseAssistant.MCP.Tools;

[McpServerToolType]
public  class Test(ServiceTest serviceTest, IOptions<Data> xd, DataBaseInfo dbInfo)
{
    
    [McpServerTool, Description("Return the available databases")]
    public async Task<string> TestService()
    {

        var xd = await serviceTest.testTheServices();
        return xd;
    }
    [McpServerTool, Description("TestIConfig")]
    public async Task<string> TestIConfiguration()
    {
        return await serviceTest.testIConfig();
    }
    
    [McpServerTool, Description("TestIOptions")]
    public async Task<ServiceTest.Embeddings> TestIOptions()
    {
        return await serviceTest.testIOptions();
    }
    [McpServerTool, Description("data test")]
    public async Task<List<Databases>> DataTest()
    {
        return await Task.FromResult(xd.Value.Databases);
    }

    [McpServerTool, Description("data test 2")]
    public async Task<List<DatabaseStatusDto>> DataTestIConfiguration()
    {
        return await dbInfo.GetDatabases();
    }
    

}