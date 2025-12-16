using Npgsql;
using RAGDatabaseAssistant.Core.Models;

namespace RAGDatabaseAssistant.Infrastructure.Database;

public class Queries
{
    public static async Task<TableSchemaDetails> GetTechnicalTableSchemaAsync(string connectionString,string tableName )
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        
        var tableSchema = new TableSchemaDetails
        {
            Name = tableName,
            Schema = "public",
            Columns = new List<ColumnInfo>(),
            Indexes = new List<IndexInfo>(),
            ForeignKeys = new List<ForeignKeyInfo>()
        };
        
        // 1. Table Information  STATUS: WORKING
        await using var colCmd = new NpgsqlCommand(@"
        SELECT 
            c.column_name,
            c.data_type,
            c.is_nullable,
            c.column_default,
            c.character_maximum_length,
            c.numeric_precision,
            c.numeric_scale,
            c.is_identity
        FROM information_schema.columns c
        WHERE c.table_schema = 'public' 
            AND lower(c.table_name) = lower(@tableName)",
            conn);
        
        colCmd.Parameters.AddWithValue("@tableName", tableName);
        await using (var reader = await colCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var column = new ColumnInfo
                {
                    Name = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2) == "YES",
                    DefaultValue = reader.IsDBNull(3) ? null : reader.GetString(3),
                    MaxLength = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Precision = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    Scale = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    IsIdentity = !reader.IsDBNull(7) && reader.GetString(7) == "YES",
                    IsComputed = false
                    //Description = reader.IsDBNull(8) ? null : reader.GetString(8)
                };
                
                tableSchema.Columns.Add(column);
            }
        }

      
        // 2. Obtener Primary Key
        await using var pkCmd = new NpgsqlCommand(@"
            SELECT 
                tc.constraint_name,
                array_agg(kcu.column_name ORDER BY kcu.ordinal_position) as columns
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu 
                ON tc.constraint_name = kcu.constraint_name
                AND tc.table_schema = kcu.table_schema
            WHERE tc.constraint_type = 'PRIMARY KEY'
                AND tc.table_schema = 'public'
                AND lower(tc.table_name) = lower(@tableName)
            GROUP BY tc.constraint_name",
            conn);
        
        pkCmd.Parameters.AddWithValue("@tableName", tableName);
        
        await using (var reader = await pkCmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                tableSchema.PrimaryKey = new PrimaryKeyInfo
                {
                    Name = reader.GetString(0),
                    Columns = ((string[])reader.GetValue(1)).ToList()
                };
            }
        }
        // 3. Obtener Foreign Keys
        await using var fkCmd = new NpgsqlCommand(@"
            SELECT
                tc.constraint_name,
                kcu.column_name,
                ccu.table_name AS foreign_table_name,
                ccu.column_name AS foreign_column_name,
                rc.delete_rule,
                rc.update_rule
            FROM information_schema.table_constraints AS tc
            JOIN information_schema.key_column_usage AS kcu
                ON tc.constraint_name = kcu.constraint_name
                AND tc.table_schema = kcu.table_schema
            JOIN information_schema.constraint_column_usage AS ccu
                ON ccu.constraint_name = tc.constraint_name
                AND ccu.table_schema = tc.table_schema
            JOIN information_schema.referential_constraints AS rc
                ON rc.constraint_name = tc.constraint_name
            WHERE tc.constraint_type = 'FOREIGN KEY'
                AND lower(tc.table_name)= lower(@tableName)
                AND tc.table_schema= 'public'",
            conn);
        
        fkCmd.Parameters.AddWithValue("@tableName", tableName);
        
        await using (var reader = await fkCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var fk = new ForeignKeyInfo
                {
                    Name = reader.GetString(0),
                    ColumnName = reader.GetString(1),
                    ReferencedTable = reader.GetString(2),
                    ReferencedColumn = reader.GetString(3),
                    OnDelete = reader.GetString(4),
                    OnUpdate = reader.GetString(5)
                };
                
                tableSchema.ForeignKeys.Add(fk);
            }
        }


        // 4. Obtener Índices
        await using var idxCmd = new NpgsqlCommand(@"
            SELECT
                i.relname AS index_name,
                a.attname AS column_name,
                ix.indisunique AS is_unique,
                ix.indisprimary AS is_primary,
                am.amname AS index_type,
                pg_get_indexdef(ix.indexrelid) AS definition,
                pg_relation_size(i.oid) AS size_bytes,
                -- Detectar si este índice está sobre una FK
                CASE 
                    WHEN EXISTS (
                        SELECT 1 FROM information_schema.table_constraints tc
                        JOIN information_schema.key_column_usage kcu 
                            ON tc.constraint_name = kcu.constraint_name
                        WHERE tc.constraint_type = 'FOREIGN KEY'
                            AND tc.table_name = t.relname
                            AND kcu.column_name = a.attname
                    ) THEN true
                    ELSE false
                END AS is_foreign_key_column
            FROM pg_class t
            JOIN pg_index ix ON t.oid = ix.indrelid
            JOIN pg_class i ON i.oid = ix.indexrelid
            JOIN pg_am am ON i.relam = am.oid
            JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY(ix.indkey)
            WHERE lower(t.relname) = lower(@tableName)
                AND t.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'public')
            ORDER BY i.relname, a.attnum",
            conn);
        
        idxCmd.Parameters.AddWithValue("@tableName", tableName);
        
        var indexDict = new Dictionary<string, IndexInfo>();
        
        await using (var reader = await idxCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var indexName = reader.GetString(0);
                var columnName = reader.GetString(1);
                
                if (!indexDict.ContainsKey(indexName))
                {
                    var indexType = reader.GetString(4).ToLower() switch
                    {
                        "btree" => IndexType.BTree,
                        "hash" => IndexType.Hash,
                        "gist" => IndexType.GiST,
                        "gin" => IndexType.GIN,
                        "hnsw" => IndexType.HNSW,
                        "ivfflat" => IndexType.IVFFlat,
                        _ => IndexType.BTree
                    };
                    
                    indexDict[indexName] = new IndexInfo
                    {
                        Name = indexName,
                        TableName = tableName,
                        Columns = new List<string>(),
                        IsUnique = reader.GetBoolean(2),
                        IsPrimaryKey = reader.GetBoolean(3),
                        Type = indexType,
                        Definition = reader.GetString(5),
                        SizeInBytes = reader.GetInt64(6),
                        isForeignKeyColumn = reader.GetBoolean(7),
                    };
                }
                
                indexDict[indexName].Columns.Add(columnName);
            }
        }
        
        tableSchema.Indexes = indexDict.Values.ToList();
        
        // 5. Obtener conteo de filas (estimado)
        await using var countCmd = new NpgsqlCommand(@"
            SELECT reltuples::bigint AS row_count
            FROM pg_class
            WHERE lower(relname) = lower(@tableName)
                AND relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'public')",
            conn);
        
        countCmd.Parameters.AddWithValue("@tableName", tableName);
        
        var rowCountObj = await countCmd.ExecuteScalarAsync();
        if (rowCountObj != null && rowCountObj != DBNull.Value)
        {
            tableSchema.RowCount = Convert.ToInt64(rowCountObj);
        }
        
        // 6. Obtener descripción de la tabla
        await using var descCmd = new NpgsqlCommand(@"
            SELECT obj_description(
                (SELECT oid FROM pg_class WHERE lower(relname) = lower(@tableName) 
                 AND relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'public'))
            )",
            conn);
        
        descCmd.Parameters.AddWithValue("@tableName", tableName);
        
        var description = await descCmd.ExecuteScalarAsync();
        if (description != null && description != DBNull.Value)
        {
            tableSchema.Description = description.ToString();
        }
        
        return tableSchema;
    }
}