using FIK.ORM.Infrastructures.Transactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace FIK.ORM.Infrastructures.MetaData;

internal class MetaDataProviderSQL : IMetaDataProvider
{
    //private static readonly Lazy<MetaDataProvider> _instance = new Lazy<MetaDataProvider>(() => new MetaDataProvider());
    private static readonly ConcurrentDictionary<string, IEnumerable<MetaDataInfo> > validTablesWithColumns = new ConcurrentDictionary<string, IEnumerable<MetaDataInfo>>();
    private readonly IDbConnection connection;
    private readonly ITransactionManager _transactionManager;

    public MetaDataProviderSQL(IDbConnection connection)
    {
        this.connection = connection;
        _transactionManager = new TransactionManager(connection);

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
            string onlyTableName = GetOnlyTableName(tableName);

            var identityColumns = GetIdentityColumns(schemaName, tableName);

            _transactionManager.ExecuteInTransaction(scope =>
            {
                using (IDbCommand oCmd = connection.CreateCommand())
                {
                    oCmd.Transaction = scope.Transaction;
                    oCmd.CommandText = "SELECT COLUMN_NAME,ORDINAL_POSITION,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION,NUMERIC_PRECISION_RADIX,NUMERIC_SCALE " +
                    "FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA=@TABLE_SCHEMA ";
                    var parameter = oCmd.CreateParameter();
                    parameter.ParameterName = "@TableName";
                    parameter.Value = onlyTableName;
                    oCmd.Parameters.Add(parameter);

                    parameter = oCmd.CreateParameter();
                    parameter.ParameterName = "@TABLE_SCHEMA";
                    parameter.Value = schemaName;
                    oCmd.Parameters.Add(parameter);

                    var columns = new List<MetaDataInfo>();
                    using (var reader = oCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columns.Add(new MetaDataInfo
                            (
                                GetValueOrDefault<string>(reader, "COLUMN_NAME"), //ColumnName
                                GetValueOrDefault<int>(reader, "ORDINAL_POSITION"),//OrdinalPosition
                                GetValueOrDefault<string>(reader, "COLUMN_DEFAULT"), //ColumnDefault 
                                GetValueOrDefault<string>(reader, "IS_NULLABLE"), //IsNullable
                                GetValueOrDefault<string>(reader, "DATA_TYPE"), //DataType
                                GetValueOrDefault<int?>(reader, "CHARACTER_MAXIMUM_LENGTH"), //CharacterMaximumLength
                                GetValueOrDefault<int?>(reader, "NUMERIC_PRECISION"), //NumericPrecision
                                GetValueOrDefault<int?>(reader, "NUMERIC_PRECISION_RADIX"), //NumericPrecisionRadix
                                GetValueOrDefault<int?>(reader, "NUMERIC_SCALE"), //NumericScale
                                identityColumns.Any(c => c.Equals(reader.GetString(0), StringComparison.OrdinalIgnoreCase)) //IdentityColumn
                            ));
                        }
                    }
                    scope.Commit();
                    validTablesWithColumns.TryAdd(tableName, columns);


                }

                
            });
            
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve metadata for table '{tableName}'.", ex);
        }
    }

    private static T GetValueOrDefault<T>(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return default(T);

        object value = reader.GetValue(ordinal);

        if (value is T typedValue)
            return typedValue;

        Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        object converted = Convert.ChangeType(value, targetType);

        return (T)converted;
    }

    private string[] GetIdentityColumns(string schemaName, string tableName)
    {
        var columns = new List<string>();

        try
        {
            tableName = GetOnlyTableName(tableName);

            _transactionManager.ExecuteInTransaction(scope =>
            {
                using (IDbCommand oCmd = connection.CreateCommand())
                {
                    oCmd.Transaction = scope.Transaction;
                    oCmd.CommandTimeout = 0; //todo: make it configurable

                    oCmd.CommandText = @"SELECT 
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
                    var parameter = oCmd.CreateParameter();
                    parameter.ParameterName = "@TableName";
                    parameter.Value = tableName;
                    oCmd.Parameters.Add(parameter);

                    parameter = oCmd.CreateParameter();
                    parameter.ParameterName = "@SchemaName";
                    parameter.Value = schemaName;
                    oCmd.Parameters.Add(parameter);

                    using (var reader = oCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columns.Add(reader.GetString(2));
                        }
                    }
                    scope.Commit();

                }
            });

            
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve metadata for table '{tableName}'.", ex);
        }

        return columns.ToArray();
    }

    private string GetOnlyTableName(string fullTableName)
    {
        if (fullTableName.Contains("."))
        {
            return fullTableName.Split('.').Last();
        }
        return fullTableName;
    }
}
