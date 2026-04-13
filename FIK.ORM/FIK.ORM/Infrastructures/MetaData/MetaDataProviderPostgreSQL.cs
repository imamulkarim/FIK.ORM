using FIK.ORM.Extensions;
using FIK.ORM.Infrastructures.Transactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FIK.ORM.Infrastructures.MetaData;

internal class MetaDataProviderPostgreSQL : IMetaDataProvider
{
    //private static readonly Lazy<MetaDataProvider> _instance = new Lazy<MetaDataProvider>(() => new MetaDataProvider());
    private static readonly ConcurrentDictionary<string, IEnumerable<MetaDataInfo>> validTablesWithColumns = new ConcurrentDictionary<string, IEnumerable<MetaDataInfo>>();
    private readonly IDbConnection connection;
    private readonly ITransactionManager _transactionManager;

    public MetaDataProviderPostgreSQL(IDbConnection connection)
    {
        this.connection = connection;
        _transactionManager = new TransactionManager(connection);
    }

    public bool IsValidTable(string schemaName, string tableName)
    {
        if (!validTablesWithColumns.ContainsKey(tableName))
        {
            RetriveTableMetaData(schemaName, tableName);
        }
        return true;
    }

    public IEnumerable<MetaDataInfo> GetTableMetaData(string schemaName, string tableName, out IEnumerable<MetaDataInfo> columns)
    {
        if (!validTablesWithColumns.TryGetValue(tableName, out columns!))
        {
            RetriveTableMetaData(schemaName, tableName);
        }
        validTablesWithColumns.TryGetValue(tableName, out columns!);
        return columns!;
    }

    public void RetriveTableMetaData(string schemaName, string tableName)
    {
        try
        {
            string onlyTableName = tableName.GetOnlyTableName().Trim('\"');

            //var identityColumns = GetIdentityColumns(schemaName, tableName);

            _transactionManager.ExecuteInTransaction(scope =>
            {
                using (IDbCommand oCmd = connection.CreateCommand())
                {
                    oCmd.Transaction = scope.Transaction;
                    oCmd.CommandText = @"SELECT * 
                                        FROM information_schema.columns 
                                        WHERE LOWER(table_schema) = LOWER(@TABLE_SCHEMA)
                                          AND LOWER(table_name) = LOWER(@TableName)
                                        ORDER BY ordinal_position;
                                         ";
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
                                reader.GetValueOrDefault<string>("column_name"), //ColumnName
                                reader.GetValueOrDefault<int>("ordinal_position"),//OrdinalPosition
                                reader.GetValueOrDefault<string>("column_default"), //ColumnDefault 
                                reader.GetValueOrDefault<string>("is_nullable"), //IsNullable
                                reader.GetValueOrDefault<string>("data_type"), //DataType
                                reader.GetValueOrDefault<int?>("character_maximum_length"), //CharacterMaximumLength
                                reader.GetValueOrDefault<int?>("NUMERIC_PRECISION"), //NumericPrecision
                                reader.GetValueOrDefault<int?>("NUMERIC_PRECISION_RADIX"), //NumericPrecisionRadix
                                reader.GetValueOrDefault<int?>("NUMERIC_SCALE"), //NumericScale
                                reader.GetValueOrDefault<string>("is_identity").Equals("YES", StringComparison.OrdinalIgnoreCase) //IdentityColumn
                            //identityColumns.Any(c => c.Equals(reader.GetString(0), StringComparison.OrdinalIgnoreCase)) //IdentityColumn
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

    private string[] GetIdentityColumns(string schemaName, string tableName)
    {
        var columns = new List<string>();

        try
        {
            tableName = tableName.GetOnlyTableName();

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

}
