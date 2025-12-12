namespace RAGDatabaseAssistant.MCP.Common;

public abstract class McpResponse
{
    public string Status { get; set; } 
}

public class McpSuccessResponse : McpResponse
{
    public McpSuccessResponse()
    {
        Status = "success";
    }
    public object Data { get; set; } 
}

public class McpErrorResponse : McpResponse
{
    public McpErrorResponse()
    {
        Status = "error";
    }
    public int Code { get; set; } 
    public string Message { get; set; } 
    public object Details { get; set; } 
}

// public class TestConnectionData
// {
//     public string DatabaseName { get; set; }
//     public string ProviderType { get; set; }
//     public bool Connected { get; set; }
//     public string ConnectionMessage { get; set; } // Mensaje informativo
// }