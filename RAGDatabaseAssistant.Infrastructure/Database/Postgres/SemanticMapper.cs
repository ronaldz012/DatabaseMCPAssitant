using RAGDatabaseAssistant.Core.Models;
using System.Text.RegularExpressions;

namespace RAGDatabaseAssistant.Infrastructure.Database;

public class SemanticMapper
{
    private static readonly Dictionary<string, string[]> CategoryKeywords = new()
    {
        ["Identity"] = new[] { "user", "account", "profile", "person", "customer", "client" },
        ["Authorization"] = new[] { "role", "permission", "policy", "access", "grant", "privilege" },
        ["Audit"] = new[] { "log", "audit", "history", "event", "track", "change" },
        ["Commerce"] = new[] { "order", "product", "cart", "payment", "invoice", "transaction", "sale" },
        ["Content"] = new[] { "post", "article", "comment", "media", "document", "file", "page" },
        ["Communication"] = new[] { "message", "notification", "email", "chat", "conversation" },
        ["Configuration"] = new[] { "setting", "config", "parameter", "option", "preference" },
        ["Location"] = new[] { "address", "location", "country", "city", "region", "zone" },
        ["Temporal"] = new[] { "schedule", "calendar", "appointment", "booking", "reservation" }
    };

    private static readonly Dictionary<string, string[]> SensitivePatterns = new()
    {
        ["Credentials"] = new[] { "password", "hash", "salt", "secret", "token", "key", "credential" },
        ["Personal"] = new[] { "ssn", "social_security", "ci", "dni", "passport", "tax_id", "cvv", "pin" },
        ["Financial"] = new[] { "card_number", "account_number", "routing", "iban", "swift" },
        ["Health"] = new[] { "medical", "diagnosis", "prescription", "health_record" }
    };

    private static readonly Dictionary<string, string[]> RolePatterns = new()
    {
        ["identifier"] = new[] { "id", "_id", "uuid", "guid", "code", "number", "identifier" },
        ["audit"] = new[] { "created", "updated", "modified", "deleted", "changed", "timestamp" },
        ["status"] = new[] { "status", "state", "is_", "has_", "active", "enabled", "verified" },
        ["security"] = new[] { "permission", "access", "role", "grant", "scope" },
        ["metadata"] = new[] { "version", "revision", "sequence", "order", "priority", "rank" },
        ["measurement"] = new[] { "count", "total", "amount", "quantity", "price", "cost", "value" }
    };

    private static readonly HashSet<string> CommonIdSuffixes = new() 
    { 
        "id", "userid", "accountid", "clientid", "customerid", "productid", "orderid" 
    };

    public static EntitySchema MapEntity(TableSchemaDetails table)
    {
        var tableName = table.Name.ToLower();
        var category = GuessCategory(tableName);

        var entity = new EntitySchema
        {
            Name = table.Name,
            Description = table.Description ?? InferDescription(table.Name, category),
            Category = category,
            Fields = table.Columns
                .Select(MapField)
                .Where(f => !f.IsSensitive || ShouldIncludeSensitive(f))
                .ToList(),
            Relations = MapRelations(table),
            CommonMetrics = GenerateMetrics(table),
            CommonQuestions = GenerateQuestions(table)
        };

        return entity;
    }

    private static SemanticField MapField(ColumnInfo col)
    {
        var semanticType = InferSemanticType(col);
        var role = InferRole(col);

        return new SemanticField
        {
            Name = col.Name,
            Nullable = col.IsNullable,
            Description = col.Description ?? InferFieldDescription(col),
            Type = semanticType,
            Role = role,
            IsSensitive = IsSensitive(col.Name),
            EnumValues = InferEnum(col)
        };
    }

    private static string InferSemanticType(ColumnInfo col)
    {
        var name = col.Name.ToLower();
        var dataType = col.DataType.ToLower();

        // IDs
        if (CommonIdSuffixes.Any(suffix => name == suffix || name.EndsWith(suffix)))
            return "id";

        // Fechas y tiempo
        if (dataType.Contains("timestamp") || dataType.Contains("date") || dataType.Contains("time"))
            return "datetime";

        // Booleanos
        if (dataType == "boolean" || dataType == "bool" || name.StartsWith("is_") || name.StartsWith("has_"))
            return "boolean";

        // Enumeraciones (basado en constraints o patrones)
        if (name.Contains("status") || name.Contains("type") || name.Contains("state") || name.Contains("role"))
            return "enum";

        // Números
        if (dataType.Contains("int") || dataType.Contains("numeric") || dataType.Contains("decimal") || 
            dataType.Contains("float") || dataType.Contains("double") || dataType.Contains("money"))
            return "number";

        // Texto
        if (dataType.Contains("text") || dataType.Contains("char") || dataType.Contains("varchar") || dataType.Contains("string"))
            return "string";

        // JSON/Arrays
        if (dataType.Contains("json") || dataType.Contains("jsonb"))
            return "json";

        if (dataType.Contains("array") || dataType.Contains("[]"))
            return "array";

        // UUID
        if (dataType.Contains("uuid"))
            return "id";

        return "string";
    }

    private static string InferRole(ColumnInfo col)
    {
        var name = col.Name.ToLower();

        foreach (var (role, patterns) in RolePatterns)
        {
            if (patterns.Any(pattern => name.Contains(pattern)))
                return role;
        }

        return "content";
    }

    private static bool IsSensitive(string name)
    {
        var lowerName = name.ToLower();

        foreach (var patterns in SensitivePatterns.Values)
        {
            if (patterns.Any(pattern => lowerName.Contains(pattern)))
                return true;
        }

        return false;
    }

    private static bool ShouldIncludeSensitive(SemanticField field)
    {
        // Incluir campos sensibles de tipo boolean o status para queries
        return field.Type == "boolean" && field.Role == "status";
    }

    private static Dictionary<int, string>? InferEnum(ColumnInfo col)
    {
        var name = col.Name.ToLower();

        // Patrones comunes de enums
        if (name.Contains("status"))
        {
            return new Dictionary<int, string>
            {
                [0] = "Inactive",
                [1] = "Active",
                [2] = "Pending",
                [3] = "Suspended"
            };
        }

        if (name.Contains("state") && !name.Contains("address"))
        {
            return new Dictionary<int, string>
            {
                [0] = "Draft",
                [1] = "Published",
                [2] = "Archived"
            };
        }

        return null;
    }

    private static List<SemanticRelation> MapRelations(TableSchemaDetails table)
    {
        var relations = new List<SemanticRelation>();

        // Mapear foreign keys
        foreach (var fk in table.ForeignKeys)
        {
            var relationType = InferRelationType(table, fk);
            
            relations.Add(new SemanticRelation
            {
                Type = relationType,
                TargetEntity = fk.ReferencedTable,
                Field = fk.ColumnName,
                Description = GenerateRelationDescription(table.Name, fk, relationType)
            });
        }

        return relations;
    }

    private static string InferRelationType(TableSchemaDetails table, ForeignKeyInfo fk)
    {
        var tableName = table.Name.ToLower();
        var columnName = fk.ColumnName.ToLower();
        var referencedTable = fk.ReferencedTable.ToLower();
        
        // 1. Detectar tablas de unión (many-to-many)
        // Características: nombre compuesto, múltiples FKs, pocas columnas adicionales
        if (IsJunctionTable(table, tableName))
        {
            return "many-to-many";
        }
        
        // 2. Detectar one-to-one
        // Si la columna FK tiene índice único y no es nullable, probablemente es 1:1
        var fkColumn = table.Columns.FirstOrDefault(c => c.Name.Equals(fk.ColumnName, StringComparison.OrdinalIgnoreCase));
        var hasUniqueIndex = table.Indexes.Any(idx => 
            idx.IsUnique && 
            idx.Columns.Count == 1 && 
            idx.Columns[0].Equals(fk.ColumnName, StringComparison.OrdinalIgnoreCase));
        
        if (hasUniqueIndex && fkColumn?.IsNullable == false)
        {
            return "one-to-one";
        }
        
        // 3. Por defecto: many-to-one
        // La tabla actual (many) apunta a la tabla referenciada (one)
        return "many-to-one";
    }

    private static bool IsJunctionTable(TableSchemaDetails table, string tableName)
    {
        // Una tabla de unión típicamente:
        // 1. Tiene exactamente 2 foreign keys
        if (table.ForeignKeys.Count != 2)
            return false;
        
        // 2. Tiene un nombre compuesto de dos entidades (UserRoles, RolePermissions, etc.)
        var hasCompoundName = tableName.Contains("_") || 
                             (char.IsUpper(table.Name[0]) && table.Name.Skip(1).Any(char.IsUpper));
        
        // 3. Tiene pocas columnas aparte de los FKs e Id (máximo 5-6 columnas adicionales)
        var fkColumnNames = table.ForeignKeys.Select(fk => fk.ColumnName.ToLower()).ToHashSet();
        var additionalColumns = table.Columns
            .Where(c => !fkColumnNames.Contains(c.Name.ToLower()))
            .Where(c => !c.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            .Where(c => !IsAuditField(c.Name))
            .Count();
        
        // 4. No tiene columnas de "contenido" sustanciales
        var hasContentColumns = table.Columns.Any(c => 
            (c.DataType.Contains("text") || c.DataType.Contains("varchar")) &&
            !IsAuditField(c.Name) &&
            !fkColumnNames.Contains(c.Name.ToLower()) &&
            !c.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        
        return hasCompoundName && additionalColumns <= 3 && !hasContentColumns;
    }

    private static bool IsAuditField(string fieldName)
    {
        var lower = fieldName.ToLower();
        return lower.Contains("created") || 
               lower.Contains("updated") || 
               lower.Contains("modified") || 
               lower.Contains("deleted");
    }

    private static string GenerateRelationDescription(string tableName, ForeignKeyInfo fk, string relationType)
    {
        var fromEntity = Humanize(tableName);
        var toEntity = Humanize(fk.ReferencedTable);
        var fromSingular = Singularize(fromEntity);
        var toSingular = Singularize(toEntity);
        var fieldName = Humanize(fk.ColumnName.Replace("_id", "").Replace("Id", ""));

        return relationType switch
        {
            "one-to-one" => $"Each {fromSingular} has exactly one {toSingular}",
            "many-to-one" => $"Each {fromSingular} belongs to one {toSingular}",
            "many-to-many" => $"{fromEntity} are associated with multiple {toEntity}",
            _ => $"{fromSingular} references {toSingular}"
        };
    }

    private static string GuessCategory(string tableName)
    {
        var lower = tableName.ToLower();

        foreach (var (category, keywords) in CategoryKeywords)
        {
            if (keywords.Any(keyword => lower.Contains(keyword)))
                return category;
        }

        return "Core";
    }

    private static string InferDescription(string tableName, string category)
    {
        var humanized = Humanize(tableName);
        var singular = Singularize(humanized);

        return category switch
        {
            "Identity" => $"Stores {humanized.ToLower()} information and credentials",
            "Authorization" => $"Defines {humanized.ToLower()} and access control rules",
            "Audit" => $"Tracks {humanized.ToLower()} for compliance and monitoring",
            "Commerce" => $"Manages {humanized.ToLower()} and related transactions",
            "Content" => $"Contains {humanized.ToLower()} and associated metadata",
            "Communication" => $"Handles {humanized.ToLower()} between users",
            "Configuration" => $"Stores application {humanized.ToLower()}",
            _ => $"Manages {humanized.ToLower()}"
        };
    }

    private static string InferFieldDescription(ColumnInfo col)
    {
        var name = col.Name.ToLower();
        var humanized = Humanize(col.Name);

        if (name.Contains("created"))
            return $"Timestamp when the record was created";
        if (name.Contains("updated") || name.Contains("modified"))
            return $"Timestamp of the last modification";
        if (name.Contains("deleted"))
            return $"Timestamp when the record was soft-deleted";
        if (name == "id")
            return "Unique identifier for this record";
        if (name.EndsWith("_id") || name.EndsWith("id"))
            return $"Reference to related {Humanize(name.Replace("_id", "").Replace("id", ""))}";
        if (name.StartsWith("is_"))
            return $"Indicates whether the record {humanized.Replace("Is ", "").ToLower()}";
        if (name.StartsWith("has_"))
            return $"Indicates whether the record {humanized.Replace("Has ", "").ToLower()}";
        if (name.Contains("count"))
            return $"Number of {humanized.Replace("Count", "").Trim().ToLower()}";
        if (name.Contains("total"))
            return $"Total {humanized.Replace("Total", "").Trim().ToLower()}";

        return humanized;
    }

    private static List<string> GenerateMetrics(TableSchemaDetails table)
    {
        var metrics = new List<string>();
        var entityName = Humanize(table.Name);
        var hasDateFields = table.Columns.Any(c => c.DataType.Contains("timestamp") || c.DataType.Contains("date"));
        var hasStatusField = table.Columns.Any(c => c.Name.ToLower().Contains("status"));
        var hasAmountField = table.Columns.Any(c => c.Name.ToLower().Contains("amount") || c.Name.ToLower().Contains("total"));

        // Métrica básica de conteo
        metrics.Add($"Total {entityName.ToLower()} count");

        if (hasDateFields)
        {
            metrics.Add($"{entityName} created per day/week/month");
            metrics.Add($"{entityName} growth trend");
        }

        if (hasStatusField)
        {
            metrics.Add($"{entityName} by status");
            metrics.Add($"Active vs inactive {entityName.ToLower()}");
        }

        if (hasAmountField)
        {
            metrics.Add($"Total and average amounts");
            metrics.Add($"Amount distribution");
        }

        if (table.ForeignKeys.Any())
        {
            var mainRelation = table.ForeignKeys.First();
            var relatedEntity = Humanize(mainRelation.ReferencedTable);
            metrics.Add($"{entityName} per {relatedEntity.ToLower()}");
        }

        return metrics;
    }

    private static List<string> GenerateQuestions(TableSchemaDetails table)
    {
        var questions = new List<string>();
        var entityName = Humanize(table.Name);
        var entityLower = entityName.ToLower();

        questions.Add($"How many {entityLower} exist?");
        questions.Add($"Show me all {entityLower}");

        var hasCreatedDate = table.Columns.Any(c => c.Name.ToLower().Contains("created"));
        if (hasCreatedDate)
        {
            questions.Add($"How many {entityLower} were created in the last 30 days?");
            questions.Add($"Show me recent {entityLower}");
        }

        var statusField = table.Columns.FirstOrDefault(c => c.Name.ToLower().Contains("status"));
        if (statusField != null)
        {
            questions.Add($"How many active {entityLower}?");
            questions.Add($"What is the status distribution of {entityLower}?");
        }

        if (table.ForeignKeys.Any())
        {
            var mainRelation = table.ForeignKeys.First();
            var relatedEntity = Humanize(mainRelation.ReferencedTable).ToLower();
            questions.Add($"How many {entityLower} does each {relatedEntity} have?");
        }

        var nameField = table.Columns.FirstOrDefault(c => 
            c.Name.ToLower() == "name" || c.Name.ToLower().Contains("title"));
        if (nameField != null)
        {
            questions.Add($"Find {entityLower} by name");
        }

        return questions;
    }

    private static string Humanize(string text)
    {
        // Convertir snake_case o PascalCase a palabras separadas
        text = Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
        text = text.Replace("_", " ");
        
        // Capitalizar primera letra de cada palabra
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
    }

    private static string Singularize(string text)
    {
        // Reglas simples de singularización
        if (text.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
            return text[..^3] + "y";
        if (text.EndsWith("es", StringComparison.OrdinalIgnoreCase))
            return text[..^2];
        if (text.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            return text[..^1];
        
        return text;
    }
}