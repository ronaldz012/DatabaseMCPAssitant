namespace RAGDatabaseAssistant.Core.Models;

/// <summary>
/// Representa una query generada por IA
/// </summary>
public class GeneratedQuery
{
    public string OriginalRequest { get; set; } = string.Empty;
    public string GeneratedSQL { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<SimilarQuery> SimilarQueriesUsed { get; set; } = new();
    public DatabaseType DatabaseType { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? Explanation { get; set; }
    public List<string> Assumptions { get; set; } = new();
    
    /// <summary>
    /// Indica si la query necesita revisión manual
    /// </summary>
    public bool NeedsReview => Confidence < 0.7;
}

/// <summary>
/// Query similar encontrada en el historial
/// </summary>
public class SimilarQuery
{
    public string Query { get; set; } = string.Empty;
    public double Similarity { get; set; }
    public QueryMetadata Metadata { get; set; } = new();
    public QueryResult? PastResult { get; set; }
}

/// <summary>
/// Metadata de una query ejecutada previamente
/// </summary>
public class QueryMetadata
{
    public DateTime Timestamp { get; set; }
    public string DatabaseType { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public int RowsReturned { get; set; }
    public bool WasSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string? UserContext { get; set; } // Quién ejecutó la query
}