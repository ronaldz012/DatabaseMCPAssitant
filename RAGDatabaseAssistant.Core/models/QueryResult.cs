namespace RAGDatabaseAssistant.Core.Models;

/// <summary>
/// Resultado de ejecutar una query
/// </summary>
public class QueryResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public int RowCount => Rows.Count;
    public TimeSpan ExecutionTime { get; set; }
    public QueryStatistics? Statistics { get; set; }
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// Convierte el resultado a formato JSON-friendly
    /// </summary>
    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            success = Success,
            rowCount = RowCount,
            executionTime = ExecutionTime.TotalMilliseconds,
            columns = Columns,
            rows = Rows,
            error = ErrorMessage
        }, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
    
    /// <summary>
    /// Convierte el resultado a formato tabla ASCII
    /// </summary>
    public string ToTable()
    {
        if (!Success)
            return $"Error: {ErrorMessage}";
            
        if (Rows.Count == 0)
            return "No rows returned.";
        
        var sb = new System.Text.StringBuilder();
        
        // Header
        sb.AppendLine(string.Join(" | ", Columns));
        sb.AppendLine(new string('-', Columns.Sum(c => c.Length) + (Columns.Count - 1) * 3));
        
        // Rows
        foreach (var row in Rows)
        {
            var values = Columns.Select(col => 
                row.ContainsKey(col) ? row[col]?.ToString() ?? "NULL" : "NULL");
            sb.AppendLine(string.Join(" | ", values));
        }
        
        sb.AppendLine($"\n{RowCount} row(s) in {ExecutionTime.TotalMilliseconds:F2}ms");
        
        return sb.ToString();
    }
}