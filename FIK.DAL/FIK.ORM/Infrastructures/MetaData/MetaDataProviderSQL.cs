using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FIK.ORM.Infrastructures.MetaData;

internal class MetaDataProviderSQL : IMetaDataProvider
{
    //private static readonly Lazy<MetaDataProvider> _instance = new Lazy<MetaDataProvider>(() => new MetaDataProvider());
    private static readonly ConcurrentDictionary<string, IEnumerable<MetaDataInfo> > validTablesWithColumns = new ConcurrentDictionary<string, IEnumerable<MetaDataInfo>>();
    private readonly IDbConnection connection;

    public MetaDataProviderSQL(IDbConnection connection)
    {
        this.connection = connection;
    }

    public bool IsValidTable(string schemaName,string tableName)
    {
        if (!validTablesWithColumns.ContainsKey(tableName))
        {
            RetriveTableMetaData(schemaName,tableName);
        }
        return true;
    }

    public IEnumerable<MetaDataInfo> GetTableMetaData(string schemaName,string tableName, out IEnumerable<MetaDataInfo> columns)
    {
        if (!validTablesWithColumns.TryGetValue(tableName, out columns!))
        {
            RetriveTableMetaData(schemaName, tableName);
        }
        validTablesWithColumns.TryGetValue(tableName, out columns!);
        return columns!;
    }

    public void RetriveTableMetaData(string schemaName,string tableName)
    {
        try
        {
            var identityColumns = GetIdentityColumns(schemaName, tableName);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COLUMN_NAME,ORDINAL_POSITION,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION,NUMERIC_PRECISION_RADIX,NUMERIC_SCALE " +
                    "FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@TableName";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);

                var columns = new List<MetaDataInfo>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(new MetaDataInfo
                        (
                            reader.GetString(0), //ColumnName
                            reader.GetInt32(1),//OrdinalPosition
                            reader.IsDBNull(2) ? null! : reader.GetString(2), //ColumnDefault 
                            reader.GetString(3), //IsNullable
                            reader.GetString(4), //DataType
                            reader.IsDBNull(5) ? null : reader.GetInt32(5), //CharacterMaximumLength
                            reader.IsDBNull(6) ? (byte?)null : reader.GetByte(6), //NumericPrecision
                            reader.IsDBNull(7) ? null : reader.GetInt32(7), //NumericPrecisionRadix
                            reader.IsDBNull(8) ? null : reader.GetInt32(8), //NumericScale
                            identityColumns.Any(c=>c.Equals(reader.GetString(0), StringComparison.OrdinalIgnoreCase)) //IdentityColumn
                        ));
                    }
                }
                validTablesWithColumns.TryAdd(tableName, columns);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve metadata for table '{tableName}'.", ex);
        }
    }

    private string[] GetIdentityColumns(string schemaName, string tableName)
    {
        var columns = new List<string>();

        try
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT 
                                            SCHEMA_NAME(t.schema_id) AS SchemaName,
                                            t.name AS TableName,
                                            c.name AS ColumnName,
                                            ic.seed_value AS Seed,
                                            ic.increment_value AS Increment,
                                            ic.last_value AS LastValue
                                        FROM
                                            sys.tables AS t
                                        JOIN
                                            sys.columns AS c ON t.object_id = c.object_id
                                        JOIN
                                            sys.identity_columns AS ic ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                                        WHERE SCHEMA_NAME(t.schema_id)= @SchemaName AND t.name = @TableName
                                        ";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@TableName";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = "@SchemaName";
                parameter.Value = schemaName;
                command.Parameters.Add(parameter);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(reader.GetString(2));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve metadata for table '{tableName}'.", ex);
        }

        return columns.ToArray();
    }
}
