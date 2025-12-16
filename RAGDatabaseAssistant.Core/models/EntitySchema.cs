namespace RAGDatabaseAssistant.Core.Models;

public class EntitySchema
{
    public string Name { get; set; } = string.Empty;

    // Qué representa esta tabla en el dominio
    public string Description { get; set; } = string.Empty;

    // Usuario, Roles, Permisos, Auditoría, etc.
    public string Category { get; set; } = "General";

    public List<SemanticField> Fields { get; set; } = new();
    public List<SemanticRelation> Relations { get; set; } = new();

    // Ayuda directa al LLM
    public List<string> CommonMetrics { get; set; } = new();
    public List<string> CommonQuestions { get; set; } = new();
}
public class SemanticField
{
    public string Name { get; set; } = string.Empty;

    // string | number | boolean | datetime | enum | id
    public string Type { get; set; } = string.Empty;

    // Qué significa realmente
    public string Description { get; set; } = string.Empty;

    public bool Nullable { get; set; }

    // audit | identifier | status | security | content
    public string Role { get; set; } = "content";

    // Seguridad
    public bool IsSensitive { get; set; }

    // Enums (si aplica)
    public Dictionary<int, string>? EnumValues { get; set; }
}
public class SemanticRelation
{
    // many-to-one, one-to-many, many-to-many
    public string Type { get; set; } = string.Empty;

    public string TargetEntity { get; set; } = string.Empty;

    // Ej: UserId → Users.Id
    public string Field { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
