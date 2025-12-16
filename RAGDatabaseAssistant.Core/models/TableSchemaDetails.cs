namespace RAGDatabaseAssistant.Core.Models;

/// <summary>
/// Representa el schema de una tabla específica
/// </summary>
public class TableSchemaDetails : TableSchema
{

    public List<IndexInfo> Indexes { get; set; } = new();
    public PrimaryKeyInfo? PrimaryKey { get; set; }
    public long? RowCount { get; set; }
    public string? Description { get; set; }
}

public class TableSchema
{
    public string Name { get; set; } = string.Empty;
    public string? Schema { get; set; } // Para PostgreSQL: "public", "private", etc.
    public List<ColumnInfo> Columns { get; set; } = new();
    public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
}


/// <summary>
/// Información de una columna
/// </summary>
public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsComputed { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Información de un índice
/// </summary>
public class IndexInfo
{
    public string Name { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public IndexType Type { get; set; }
    public string? Definition { get; set; }
    public long? SizeInBytes { get; set; }
}

public enum IndexType
{
    BTree,
    Hash,
    GiST,
    GIN,
    HNSW,  // Para pgvector
    IVFFlat, // Para pgvector
    Clustered,
    NonClustered
}

/// <summary>
/// Información de foreign key
/// </summary>
public class ForeignKeyInfo
{
    public string Name { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
    public string OnDelete { get; set; } = "NO ACTION";
    public string OnUpdate { get; set; } = "NO ACTION";
}

/// <summary>
/// Información de primary key
/// </summary>
public class PrimaryKeyInfo
{
    public string Name { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
}

/// <summary>
/// Información de una vista
/// </summary>
public class ViewSchema
{
    public string Name { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public List<ColumnInfo> Columns { get; set; } = new();
}