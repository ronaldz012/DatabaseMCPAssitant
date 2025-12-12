using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RAGDatabaseAssistant.MCP.Tools;

[McpServerToolType]
public class ExampleTool
{

        [McpServerTool, Description("Returns the temperature in degrees.")]
        public async Task<string> GetWeather(string city)
        {
            //in a real case this will call an API
            var temperature = Random.Shared.Next(1, 40);
            var xd = await Task.FromResult($"the temperature in {city} is {temperature}°C");
            return xd;
        }
    

}