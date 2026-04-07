using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FIK.ORM.Infrastructures.MetaData
{
    internal class MetaDataProviderSQLite : IMetaDataProvider
    {
        //private static readonly Lazy<MetaDataProvider> _instance = new Lazy<MetaDataProvider>(() => new MetaDataProvider());
        private static readonly ConcurrentDictionary<string, IEnumerable<MetaDataInfo>> validTablesWithColumns = new ConcurrentDictionary<string, IEnumerable<MetaDataInfo>>();
        private readonly IDbConnection connection;

        public MetaDataProviderSQLite(IDbConnection connection)
        {
            this.connection = connection;
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
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "pragma table_info(@TableName); ";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@TableName";
                    parameter.Value = tableName;
                    command.Parameters.Add(parameter);

                    /*
                     * The pragma table_info returns the following columns:
                     * 0: cid (Column ID)
                     * 1: name (Column Name)
                     * 2: type (Data Type)
                     * 3: notnull (Not Null Flag)
                     * 4: dflt_value (Default Value)
                     * 5: pk (Primary Key Flag)
                     * 
                     * cid	name	type	notnull	dflt_value	pk
                        0	sId	    INTEGER	1	    NULL	    1
                        1	sName	TEXT	1	    NULL	    0
                        2	Age	    NUMERIC	0	    NULL	    0
                     */

                    var columns = new List<MetaDataInfo>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columns.Add(new MetaDataInfo
                            (
                                reader.GetString(1), //ColumnName
                                reader.GetInt32(0),//OrdinalPosition
                                reader.IsDBNull(4) ? null! : reader.GetString(4), //ColumnDefault 
                                reader.GetString(3), //IsNullable
                                reader.GetString(2), //DataType
                                null, //CharacterMaximumLength
                                null , //NumericPrecision
                                null , //NumericPrecisionRadix
                                null , //NumericScale
                                reader.GetInt32(5) == 1 && (reader.GetString(2).Equals("NUMERIC", StringComparison.OrdinalIgnoreCase) || reader.GetString(2).Equals("INTEGER", StringComparison.OrdinalIgnoreCase)) //IdentityColumn
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
    }
}
