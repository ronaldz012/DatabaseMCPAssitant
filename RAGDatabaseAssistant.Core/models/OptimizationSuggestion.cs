namespace RAGDatabaseAssistant.Core.Models;

/// <summary>
/// Sugerencia de optimización para una query
/// </summary>
public class OptimizationSuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OptimizationType Type { get; set; }
    public Priority Priority { get; set; }
    public string? ProposedChange { get; set; }
    public string? Reasoning { get; set; }
    public EstimatedImpact? Impact { get; set; }
}

public enum OptimizationType
{
    AddIndex,
    RewriteQuery,
    ChangeJoinOrder,
    AddPartitioning,
    UpdateStatistics,
    CacheResult,
    Denormalization,
    RemoveSubquery
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

public class EstimatedImpact
{
    public double PerformanceImprovement { get; set; } // Porcentaje estimado
    public string? SpeedupDescription { get; set; } // "2x faster", "50% less memory"
    public double? CostReduction { get; set; }
}
