namespace RAGDatabaseAssistant.Core.Models;

/// <summary>
/// Estadísticas de ejecución de una query
/// </summary>
public class QueryStatistics
{
    // Estimaciones del query planner
    public double EstimatedCost { get; set; }
    public int EstimatedRows { get; set; }
    public double EstimatedWidth { get; set; }
    
    // Datos reales de ejecución
    public TimeSpan? ActualTime { get; set; }
    public int? ActualRows { get; set; }
    public int? ActualLoops { get; set; }
    
    // Uso de recursos
    public long? BuffersHit { get; set; }
    public long? BuffersRead { get; set; }
    public long? BuffersWritten { get; set; }
    
    // Información de plan
    public string PlanType { get; set; } = string.Empty; // "Seq Scan", "Index Scan", etc.
    public List<string> UsedIndexes { get; set; } = new();
    public List<string> JoinTypes { get; set; } = new();
    
    // Análisis
    public bool IsSlowQuery => ActualTime?.TotalMilliseconds > 1000;
    public bool HasFullTableScan => PlanType.Contains("Seq Scan");
    public double CostAccuracy => EstimatedRows > 0 
        ? Math.Abs(1 - ((double)(ActualRows ?? 0) / EstimatedRows))
        : 0;
    
    /// <summary>
    /// Genera recomendaciones basadas en las estadísticas
    /// </summary>
    public List<string> GetRecommendations()
    {
        var recommendations = new List<string>();
        
        if (HasFullTableScan && (ActualRows ?? 0) > 1000)
        {
            recommendations.Add("⚠️ Full table scan detected on large table. Consider adding an index.");
        }
        
        if (CostAccuracy > 0.5)
        {
            recommendations.Add("⚠️ Query planner estimates are inaccurate. Consider running ANALYZE.");
        }
        
        if (BuffersRead > BuffersHit * 2)
        {
            recommendations.Add("⚠️ Low cache hit ratio. Query is reading from disk frequently.");
        }
        
        if (EstimatedCost > 10000)
        {
            recommendations.Add("⚠️ High query cost. Consider query optimization or partitioning.");
        }
        
        return recommendations;
    }
}