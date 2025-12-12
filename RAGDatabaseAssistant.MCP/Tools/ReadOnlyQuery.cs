using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RAGDatabaseAssistant.Core.Interfaces;
using RAGDatabaseAssistant.Core.Models;
using RAGDatabaseAssistant.Infrastructure.Database;
using RAGDatabaseAssistant.Infrastructure.Dtos;
using RAGDatabaseAssistant.Infrastructure.Services;
using RAGDatabaseAssistant.MCP.Common;

namespace RAGDatabaseAssistant.MCP.Tools;

[McpServerToolType]
public sealed class QueryTool( IDatabaseProviderFactory databaseProviderFactory, DataBaseInfo databaseInfo)
{
    
    
    [McpServerTool, Description("Return the available databases")]
    public async Task<List<DatabaseStatusDto>> GetAvailableDatabases()  // ← Cambiar a string
    {
        return await databaseInfo.GetDatabases();
    }
    
    [McpServerTool, Description("Test database connection")]
    public async Task<McpResponse> TestDatabaseConnection(
        [Description("Database name (e.g., MainDB, AnalyticsDB)")] string databaseName)
    {
        try
        {
            IDatabaseProvider provider = databaseProviderFactory.GetProvider(databaseName);
            var isConnected = await provider.TestConnectionAsync();
            if (isConnected)
            {
                return new McpSuccessResponse()
                {
                    Data = new
                    {
                        connected = true,
                        DatabaseName = databaseName,
                        ProviderType = provider.Type.ToString()
                    }
                };
            }

            return new McpErrorResponse()
            {
                Message = "Database not connected",
                Details = $"could not connect  to de database  {databaseName}"
            };
        }
        catch (Exception ex)
        {
            return new McpErrorResponse()
            {
                Message = ex.Message,
                Details = ex.InnerException?.Message  ?? ""
            };
        }
        
    }

    [McpServerTool, Description("Get The name of the database tables")]
    public async Task<List<string>> GetTableNames(string databaseName)
    {
        var provider = databaseProviderFactory.GetProvider(databaseName);

        var result = await provider.GetTablesAsync();
        
        return result;
    }
    [McpServerTool, Description("HealtyCheck")]
    public static string HealtyTest()
    {
        return "All good";
    }

}