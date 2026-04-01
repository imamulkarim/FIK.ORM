using FIK.ORM.Extensions;
using FIK.ORM.Infrastructures;
using FIK.ORM.Infrastructures.Data;
using FIK.ORM.Infrastructures.MetaData;
using FIK.ORM.Infrastructures.QueryBuilders;
using FIK.ORM.Infrastructures.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;


namespace FIK.ORM
{
    public class QueryExecutorClient
    {
        private readonly string _dbConnectionString;
        private readonly DatabaseProvider _databaseProvider;
        private readonly IDbConnection _IDbConnection;
        private readonly IMetaDataProvider _metaDataProvider;
        private readonly IQueryBuilder _queryBuilder;
        private readonly MetaDataValidator _metaDataValidator;
        private readonly ITransactionManager _transactionManager;

        public QueryExecutorClient(string dbConnectionString, DatabaseProvider databaseProvider)
        {
            _dbConnectionString = dbConnectionString;
            _databaseProvider = databaseProvider;
            _IDbConnection = DBObjectFactory.GetDbConnection(databaseProvider, dbConnectionString);
            _metaDataProvider = MetaDataProviderFactory.GetMetaDataProvider(databaseProvider, _IDbConnection);
            _queryBuilder = QueryBuilderFactory.GetQueryBuilder(databaseProvider, _metaDataProvider);
            _metaDataValidator = new MetaDataValidator(_metaDataProvider);
            _transactionManager = new TransactionManager(_IDbConnection);
        }

        /// <summary>
        /// Inserts the specified data object into the database table.
        /// </summary>
        /// <typeparam name="T">The type of the data object to insert.</typeparam>
        /// <param name="dataObject">The data object to insert. Must not be null.</param>
        /// <param name="columns">The columns to include in the insert operation. If null, all columns are included.</param>
        /// <param name="tableName">The name of the table to insert into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObject"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during insert.</exception>
        public void Insert<T>(T dataObject, IEnumerable<string>? columns, string tableName = "", string schemaName = "dbo") where T : class
        {
            if (dataObject == null)
            {
                throw new ArgumentException("compositeModelBuilder must not be null or empty.", nameof(dataObject));
            }

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            var insertQuery = _queryBuilder.BuildInsertQuery(dataObject.GetType(), columns, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObject.GetType(), tableName, schemaName);
                _metaDataValidator.ValidateColumns(_tableName, columns, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, false);

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                    {
                        oCmd.CommandText = insertQuery;
                        oCmd.CommandTimeout = 0; //todo: make it configurable
                        oCmd.Transaction = scope.Transaction; 
                        AddCommandParameter(oCmd, props, metaDatas, dataObject, columns);
                        oCmd.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during insert: {ex.Message}\nQuery: {insertQuery}", ex);
            }
                        
        }

        /// <summary>
        /// Inserts the specified data object into the database table.
        /// </summary>
        /// <typeparam name="T">The type of the data object to insert.</typeparam>
        /// <param name="dataObjects">The list of data object to insert. Must not be null.</param>
        /// <param name="columns">The columns to include in the insert operation. If null, all columns are included.</param>
        /// <param name="tableName">The name of the table to insert into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObjects"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during insert.</exception>
        public void InsertBatch<T>(List<T> dataObjects, IEnumerable<string>? columns, string tableName = "", string schemaName = "dbo") where T : class
        {
            
            if (dataObjects == null || dataObjects.Count == 0)
            {
                throw new ArgumentException("dataObjects must not be null or empty.", nameof(dataObjects));
            }

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            

            var insertQuery = _queryBuilder.BuildInsertQuery(dataObjects.First().GetType(), columns, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObjects.First().GetType(), tableName, schemaName);
                _metaDataValidator.ValidateColumns(_tableName, columns, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, false);

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    foreach (var dataObject in dataObjects)
                    {
                        using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                        {
                            oCmd.CommandText = insertQuery;
                            oCmd.CommandTimeout = 0;
                            oCmd.Transaction = scope.Transaction;
                            AddCommandParameter(oCmd, props, metaDatas, dataObject, columns);
                            oCmd.ExecuteNonQuery();
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during insert: {ex.Message}\nQuery: {insertQuery}", ex);
            }

        }

        /// <summary>
        /// Updates the specified data object into the database table.
        /// </summary>
        /// <typeparam name="T">The type of the data object to update.</typeparam>
        /// <param name="dataObject">The data object to update. Must not be null.</param>
        /// <param name="columns">The columns to include in the update operation. If null, all columns are included.</param>
        /// <param name="whereColumns">The whereColumns to include in the update operation. If null, no columns are included.</param>
        /// <param name="tableName">The name of the table to update into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObject"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during update.</exception>
        public void Update<T>(T dataObject, IEnumerable<string>? columns, string[]? whereColumns , string tableName = "", string schemaName = "dbo") where T : class
        {
            if (dataObject == null)
            {
                throw new ArgumentException("compositeModelBuilder must not be null or empty.", nameof(dataObject));
            }

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            var updateQuery = _queryBuilder.BuildUpdateQuery(dataObject.GetType(), columns!, whereColumns, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObject.GetType(), tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, true);
                MetaDataInfo[] metaDatasWhere = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns, true);
                //metaDatas = metaDatas.Union(metaDatasWhere).ToArray();

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                    {
                        oCmd.CommandText = updateQuery;
                        oCmd.CommandTimeout = 0; //todo: make it configurable
                        oCmd.Transaction = scope.Transaction; 
                        AddCommandParameter(oCmd, props, metaDatas, dataObject, columns);
                        AddCommandParameter(oCmd, props, metaDatasWhere,  whereColumns);
                        oCmd.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during update: {ex.Message}\nQuery: {updateQuery}", ex);
            }
                        
        }

        /// <summary>
        /// Updates the specified data object into the database table.
        /// </summary>
        /// <typeparam name="T">The type of the data object to update.</typeparam>
        /// <param name="dataObjects">The list of data object to udate. Must not be null.</param>
        /// <param name="columns">The columns to include in the update operation. If null, all columns are included.</param>
        /// <param name="whereColumns">The whereColumns to include in the update operation. If null, no columns are included.</param>
        /// <param name="tableName">The name of the table to update into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObjects"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during update.</exception>
        public void UpdateBatch<T>(List<T> dataObjects, IEnumerable<string>? columns, string[]? whereColumns, string tableName = "", string schemaName = "dbo") where T : class
        {

            if (dataObjects == null || dataObjects.Count == 0)
            {
                throw new ArgumentException("dataObjects must not be null or empty.", nameof(dataObjects));
            }

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));


            var updateQuery = _queryBuilder.BuildUpdateQuery(dataObjects.First().GetType(), columns!, whereColumns, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObjects.First().GetType(), tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, false);
                MetaDataInfo[] metaDatasWhere = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns, true);
                metaDatas = metaDatas.Union(metaDatasWhere).ToArray();

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    foreach (var dataObject in dataObjects)
                    {
                        using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                        {
                            oCmd.CommandText = updateQuery;
                            oCmd.CommandTimeout = 0;
                            oCmd.Transaction = scope.Transaction;
                            AddCommandParameter(oCmd, props, metaDatas, dataObject, whereColumns);
                            oCmd.ExecuteNonQuery();
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during update: {ex.Message}\nQuery: {updateQuery}", ex);
            }

        }

        /// <summary>
        /// Deletes the specified data object into the database table.
        /// </summary>
        /// <typeparam name="T">The type of the data object to delete.</typeparam>
        /// <param name="dataObject">The data object to delete. Must not be null.</param>
        /// <param name="whereColumns">The whereColumns to include in the delete operation.Must not be null.</param>
        /// <param name="tableName">The name of the table to delete into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObject"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during delete.</exception>
        public void Delete<T>(T dataObject, string[] whereColumns, string tableName = "", string schemaName = "dbo") where T : class
        {
            if (dataObject == null)
            {
                throw new ArgumentException("compositeModelBuilder must not be null or empty.", nameof(dataObject));
            }
            if (whereColumns == null || whereColumns.Length ==0)
            {
                throw new ArgumentException("whereColumns must not be null or empty.", nameof(whereColumns));
            }

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            var deleteQuery = _queryBuilder.BuildDeleteQuery(dataObject.GetType(),  whereColumns, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObject.GetType(), tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns, true);

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                    {
                        oCmd.CommandText = deleteQuery;
                        oCmd.CommandTimeout = 0; //todo: make it configurable
                        oCmd.Transaction = scope.Transaction;
                        AddCommandParameter(oCmd, props, metaDatas, dataObject, whereColumns);
                        oCmd.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during delete: {ex.Message}\nQuery: {deleteQuery}", ex);
            }

        }

        /// <summary>
        /// Select the specified data object from the database table.
        /// </summary>
        /// <param name="entityType">The data object to select.Must not be null.</param>
        /// <param name="columns">The columns to include in the select operation. optional</param>
        /// <param name="whereClause">The whereClause to include in the select operation.</param>
        /// <param name="orderByColumn">The orderByColumn to include in the select operation.</param>
        /// <param name="limit">The limit to include in the select operation.</param>
        /// <param name="tableName">The name of the table to delete into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityType"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during select.</exception>
        public DataTable Select(Type entityType, string[]? columns = null, string? whereClause = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo")
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(entityType);

            var selectQuery = _queryBuilder.BuildSelectQuery(entityType,  columns,whereClause,orderByColumn, limit, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(entityType, tableName, schemaName);

                return _transactionManager.ExecuteInTransaction(scope =>
                {
                    using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                    {
                        oCmd.CommandText = selectQuery;
                        oCmd.CommandTimeout = 0; //todo: make it configurable
                        oCmd.Transaction = scope.Transaction;
                        
                        IDbDataAdapter adapter = DBObjectFactory.GetDbDataAdapter(_databaseProvider);
                        adapter.SelectCommand = oCmd;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        return ds.Tables[0];
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during select: {ex.Message}\nQuery: {selectQuery}", ex);
            }
        }


        /// <summary>
        /// Select the specified data object from the database table.
        /// </summary>
        /// <param name="entityType">The data object to select.Must not be null.</param>
        /// <param name="columns">The columns to include in the select operation. optional</param>
        /// <param name="whereColumns">The whereClause to include in the select operation.</param>
        /// <param name="orderByColumn">The orderByColumn to include in the select operation.</param>
        /// <param name="limit">The limit to include in the select operation.</param>
        /// <param name="tableName">The name of the table to delete into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityType"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during select.</exception>
        public DataTable Select(Type entityType, string[]? columns = null, Dictionary<string, string>? whereColumns = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo")
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(entityType);

            var selectQuery = _queryBuilder.BuildSelectQuery(entityType, columns, whereColumns?.Select(m=>m.Key).ToArray(), orderByColumn, limit, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(entityType, tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns?.Select(m => m.Key).ToArray(), true);

                return _transactionManager.ExecuteInTransaction(scope =>
                {
                    using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                    {
                        oCmd.CommandText = selectQuery;
                        oCmd.CommandTimeout = 0; //todo: make it configurable
                        oCmd.Transaction = scope.Transaction;
                        AddCommandParameter(oCmd, props, metaDatas, whereColumns);

                        IDbDataAdapter adapter = DBObjectFactory.GetDbDataAdapter(_databaseProvider);
                        adapter.SelectCommand = oCmd;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        return ds.Tables[0];
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during select: {ex.Message}\nQuery: {selectQuery}", ex);
            }
        }


        /// <summary>
        /// Select the specified data object from the database table.
        /// </summary>
        /// <param name="entityType">The data object to select.Must not be null.</param>
        /// <param name="columns">The columns to include in the select operation. optional</param>
        /// <param name="whereColumns">The whereClause to include in the select operation.</param>
        /// <param name="orderByColumn">The orderByColumn to include in the select operation.</param>
        /// <param name="limit">The limit to include in the select operation.</param>
        /// <param name="tableName">The name of the table to delete into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityType"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during select.</exception>
        public IEnumerable<T> Select<T>(Type entityType, string[]? columns = null, Dictionary<string, string>? whereColumns = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo") where T : class, new()
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(entityType);

            var selectQuery = _queryBuilder.BuildSelectQuery(entityType, columns, whereColumns?.Select(m => m.Key).ToArray(), orderByColumn, limit, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(entityType, tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, true);
                MetaDataInfo[] metaDatasWhere = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns?.Select(m => m.Key).ToArray(), true);
                
                using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                {
                    oCmd.CommandText = selectQuery;
                    oCmd.CommandTimeout = 0; //todo: make it configurable
                    AddCommandParameter(oCmd, props, metaDatasWhere, whereColumns);
                    return _transactionManager.ExecuteInTransaction<T>(_IDbConnection, oCmd, metaDatas);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during select: {ex.Message}\nQuery: {selectQuery}", ex);
            }
        }


        /// <summary>
        /// Geneerate Insert or update query based on CompositeModel passed as parameter 
        /// using SqlCommand  Parameter add , sql injection not possible
        /// </summary>
        /// <param name="compositeModelBuilder"> pass a CompositeModel object which can be build by access CompositeModel class </param>
        /// <returns></returns>
        public void ExecuteCompositeModel(CompositeModelBuilder compositeModelBuilder)
        {
            #region query generation

            foreach (CompositeModel c in compositeModelBuilder.GetRecordSet())
            {
                PropertyDescriptorCollection props =
               TypeDescriptor.GetProperties(c.ObjectType);

                //var inst = Activator.CreateInstance(c.ObjectType);
                if (c.OperationMode != OperationMode.Custom && c.Model.Count == 0)
                {
                    throw new ArgumentException("Model must not be null or empty.", nameof(c.Model));
                }

                //string tableName = c.ObjectType.GetType().Name;


                string dynamicQuery = "";
                if (c.OperationMode == OperationMode.Update)
                {
                    var updateQuery = _queryBuilder.BuildUpdateQuery(compositeModelBuilder.GetType(), c.UpdateColumns, c.GetWhereColumns(), c.TableName, c.SchemaName);

                    c.GeneratedQuery = updateQuery;
                }
                if (c.OperationMode == OperationMode.Delete)
                {
                    var deleteQuery = _queryBuilder.BuildDeleteQuery(compositeModelBuilder.GetType(), c.GetWhereColumns(), c.TableName, c.SchemaName);
                    
                    c.GeneratedQuery = deleteQuery;
                }
                else if (c.OperationMode == OperationMode.Insert)
                {
                    var insertQuery = _queryBuilder.BuildInsertQuery(compositeModelBuilder.GetType(), c.InsertColumns, c.TableName, c.SchemaName);

                    c.GeneratedQuery = insertQuery;
                }
                else if (c.OperationMode == OperationMode.InsertOrUpdate)
                {
                    var insertQuery = _queryBuilder.BuildInsertQuery(compositeModelBuilder.GetType(), c.InsertColumns, c.TableName, c.SchemaName);

                    var updateQuery = _queryBuilder.BuildUpdateQuery(compositeModelBuilder.GetType(), c.UpdateColumns, c.GetWhereColumns(), c.TableName, c.SchemaName);
                    
                    var whereQuery = _queryBuilder.GetWhereClause(compositeModelBuilder.GetType(), c.GetWhereColumns(), c.TableName, c.SchemaName);

                    var tableName = _queryBuilder.GetTableName(compositeModelBuilder.GetType(), c.TableName, c.SchemaName);

                    dynamicQuery = string.Format(@"
                                        if exists(select * from {0} where {1} )
                                        begin
                                         {2}
                                        end
                                        else
                                        begin
                                           {3}
                                        end ", tableName, whereQuery, updateQuery, insertQuery);


                    c.GeneratedQuery = dynamicQuery;

                }
                else if (c.OperationMode == OperationMode.Custom)
                {
                    c.GeneratedQuery = c.RawQuery.SanitizeForSql();
                }
                else
                {
                    throw new ArgumentException("Query Parse failed", nameof(c.Model));
                }



            }
            #endregion

            var currentQuery = "";
            try
            {

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    foreach (CompositeModel c in compositeModelBuilder.GetRecordSet())
                    {
                        PropertyDescriptorCollection props = TypeDescriptor.GetProperties(c.ObjectType);

                        var _tableName = _metaDataValidator.GetTableName(c.ObjectType, c.TableName, c.SchemaName);
                        MetaDataInfo[] insertColumnsMetaDatas = _metaDataValidator.GetValidColumns(c.SchemaName, _tableName, c.InsertColumns, false);
                        MetaDataInfo[] updateColumnsmetaDatas = _metaDataValidator.GetValidColumns(c.SchemaName, _tableName, c.UpdateColumns, true);
                        MetaDataInfo[] whereColumnsmetaDatas = _metaDataValidator.GetValidColumns(c.SchemaName, _tableName, c.GetWhereColumns(), true);
                        
                        var metaDatasCommon = insertColumnsMetaDatas.Union(updateColumnsmetaDatas).ToArray();

                        if (c.OperationMode == OperationMode.Update)
                        {
                            foreach (var dataObject in c.Model)
                            {
                                using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                                {
                                    oCmd.CommandText = c.GeneratedQuery;
                                    currentQuery = c.GeneratedQuery;
                                    oCmd.CommandTimeout = 0;
                                    oCmd.Transaction = scope.Transaction;
                                    AddCommandParameter(oCmd, props, updateColumnsmetaDatas, dataObject, c.UpdateColumns);
                                    AddCommandParameter(oCmd, props, whereColumnsmetaDatas, c.WhereColumns);
                                    oCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        else if (c.OperationMode == OperationMode.Insert)
                        {
                            foreach (var dataObject in c.Model)
                            {
                                using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                                {
                                    oCmd.CommandText = c.GeneratedQuery;
                                    currentQuery = c.GeneratedQuery;
                                    oCmd.CommandTimeout = 0;
                                    oCmd.Transaction = scope.Transaction;
                                    AddCommandParameter(oCmd, props, insertColumnsMetaDatas, dataObject, c.InsertColumns);
                                    oCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        else if (c.OperationMode == OperationMode.Delete)
                        {
                            foreach (var dataObject in c.Model)
                            {
                                using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                                {
                                    oCmd.CommandText = c.GeneratedQuery;
                                    currentQuery = c.GeneratedQuery;
                                    oCmd.CommandTimeout = 0;
                                    oCmd.Transaction = scope.Transaction;
                                    AddCommandParameter(oCmd, props, whereColumnsmetaDatas, c.WhereColumns);
                                    oCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        else if (c.OperationMode == OperationMode.InsertOrUpdate)
                        {
                            foreach (var dataObject in c.Model)
                            {
                                using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                                {
                                    oCmd.CommandText = c.GeneratedQuery;
                                    currentQuery = c.GeneratedQuery;
                                    oCmd.CommandTimeout = 0;
                                    oCmd.Transaction = scope.Transaction;
                                    AddCommandParameter(oCmd, props, metaDatasCommon, dataObject, c.UpdateColumns.Union(c.InsertColumns!) );
                                    AddCommandParameter(oCmd, props, whereColumnsmetaDatas, c.WhereColumns);
                                    oCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        else if (c.OperationMode == OperationMode.Custom)
                        {
                            foreach (var dataObject in c.Model)
                            {
                                using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                                {
                                    oCmd.CommandText = c.GeneratedQuery;
                                    currentQuery = c.GeneratedQuery;
                                    oCmd.CommandTimeout = 0;
                                    oCmd.Transaction = scope.Transaction;
                                    oCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    

                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during update: {ex.Message}\nQuery: {currentQuery}", ex);
            }
        }


        private void AddCommandParameter(IDbCommand command, PropertyDescriptorCollection props, MetaDataInfo[] metaDatas, Dictionary<string, string>? whereColumns)
        {
            if (whereColumns == null)
                return;

            foreach (var metaData in metaDatas)
            {
                var prop = props.Find(metaData.ColumnName, true);
                if (prop == null)
                {
                    continue;
                }

                // If columns are specified, only add those columns
                var whereColumn =
                    whereColumns.First(c => string.Equals(c.Key, prop.Name, StringComparison.InvariantCultureIgnoreCase));

                var value = whereColumn.Value;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@" + prop.Name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
        }

        private void AddCommandParameter(IDbCommand command, PropertyDescriptorCollection props, MetaDataInfo[] metaDatas, object dataObject, IEnumerable<string>? columns)
        {
            foreach (var metaData in metaDatas)
            {
                var prop = props.Find(metaData.ColumnName, true);
                if (prop == null)
                {
                    continue;
                }

                // If columns are specified, only add those columns
                if (columns != null && !columns.Any(c => string.Equals(c, prop.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                var value = prop.GetValue(dataObject) ?? DBNull.Value;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@" + prop.Name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
        }

    }
}




/*
 * 
 * 
 public bool ExecuteQuery(List<string> SQL, ref string ErrorMsg)
{
    try
    {
        return _transactionManager.ExecuteInTransaction(scope =>
        {
            foreach (string s in SQL)
            {
                var cmd = new SqlCommand(s, _connection)
                {
                    Transaction = scope.Transaction
                };
                cmd.ExecuteNonQuery();
            }
            return true;
        });
    }
    catch (Exception ex)
    {
        ErrorMsg = ex.Message;
        return false;
    }
}


public async Task<bool> ExecuteQueryAsync(List<string> SQL, string errorMsg)
{
    return await _transactionManager.ExecuteInTransactionAsync(async scope =>
    {
        foreach (string s in SQL)
        {
            var cmd = new SqlCommand(s, _connection)
            {
                Transaction = scope.Transaction
            };
            await Task.FromResult(cmd.ExecuteNonQuery());
        }
        return true;
    });
}

 * 
 * */