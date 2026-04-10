using FIK.ORM.Enums;
using FIK.ORM.Extensions;
using FIK.ORM.Infrastructures.Data;
using FIK.ORM.Infrastructures.MetaData;
using FIK.ORM.Infrastructures.QueryBuilders;
using FIK.ORM.Infrastructures.Transactions;
using FIK.ORM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;


namespace FIK.ORM
{
    /// <summary>
    /// Executes CRUD operations and composite command batches against the configured database.
    /// </summary>
    public class QueryExecutorClient
    {
        private readonly string _dbConnectionString;
        private readonly DatabaseProvider _databaseProvider;
        private readonly IDbConnection _IDbConnection;
        private readonly IDbConnection _IDbConnectionSelect;
        private readonly IMetaDataProvider _metaDataProvider;
        private readonly IQueryBuilder _queryBuilder;
        private readonly MetaDataValidator _metaDataValidator;
        private readonly ITransactionManager _transactionManager;
        private readonly ITransactionManager _transactionManagerSelect;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutorClient"/> class.
        /// </summary>
        /// <param name="dbConnectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider used for connections, metadata, and commands.</param>
        public QueryExecutorClient(string dbConnectionString, DatabaseProvider databaseProvider)
        {
            _dbConnectionString = dbConnectionString;
            _databaseProvider = databaseProvider;
            _IDbConnection = DBObjectFactory.GetDbConnection(databaseProvider, dbConnectionString);
            _IDbConnectionSelect = DBObjectFactory.GetDbConnection(databaseProvider, dbConnectionString);
            _metaDataProvider = MetaDataProviderFactory.GetMetaDataProvider(databaseProvider, _IDbConnectionSelect);
            _queryBuilder = QueryBuilderFactory.GetQueryBuilder(databaseProvider, _metaDataProvider);
            _metaDataValidator = new MetaDataValidator(_metaDataProvider);
            _transactionManager = new TransactionManager(_IDbConnection);
            _transactionManagerSelect = new TransactionManager(_IDbConnectionSelect);

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
            TruncateSchemaName(ref schemaName);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            var insertQuery = _queryBuilder.BuildInsertQuery(dataObject.GetType(), columns, tableName, schemaName, _databaseProvider == DatabaseProvider.Sqlite ? true : false);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObject.GetType(), tableName, schemaName);
                _metaDataValidator.ValidateColumns(_tableName, columns, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, _databaseProvider == DatabaseProvider.Sqlite ? true : false);

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
            TruncateSchemaName(ref schemaName);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));


            var insertQuery = _queryBuilder.BuildInsertQuery(dataObjects.First().GetType(), columns, tableName, schemaName, _databaseProvider == DatabaseProvider.Sqlite ? true : false);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObjects.First().GetType(), tableName, schemaName);
                _metaDataValidator.ValidateColumns(_tableName, columns, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, _databaseProvider == DatabaseProvider.Sqlite ? true : false);

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
        public void Update<T>(T dataObject, IEnumerable<string>? columns, string[]? whereColumns, string tableName = "", string schemaName = "dbo") where T : class
        {
            if (dataObject == null)
            {
                throw new ArgumentException("compositeModelBuilder must not be null or empty.", nameof(dataObject));
            }
            TruncateSchemaName(ref schemaName);

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
                        AddCommandParameter(oCmd, props, metaDatasWhere, dataObject, whereColumns, true);
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
            TruncateSchemaName(ref schemaName);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));


            var updateQuery = _queryBuilder.BuildUpdateQuery(dataObjects.First().GetType(), columns!, whereColumns, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(dataObjects.First().GetType(), tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, _databaseProvider == DatabaseProvider.Sqlite ? true : false);
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
                            AddCommandParameter(oCmd, props, metaDatas, dataObject, columns);
                            AddCommandParameter(oCmd, props, metaDatas, dataObject, whereColumns, true);
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
            if (whereColumns == null || whereColumns.Length == 0)
            {
                throw new ArgumentException("whereColumns must not be null or empty.", nameof(whereColumns));
            }
            TruncateSchemaName(ref schemaName);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            var deleteQuery = _queryBuilder.BuildDeleteQuery(dataObject.GetType(), whereColumns, tableName, schemaName);

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
                        AddCommandParameter(oCmd, props, metaDatas, dataObject, whereColumns, true);
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
        /// Deletes the specified data object into the database table.
        /// </summary>
        /// <typeparam name="T">The type of the data object to delete.</typeparam>
        /// <param name="dataObjects">The data objects to delete. Must not be null.</param>
        /// <param name="whereColumns">The whereColumns to include in the delete operation.Must not be null.</param>
        /// <param name="tableName">The name of the table to delete into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObject"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during delete.</exception>
        public void DeleteBatch<T>(List<T> dataObjects, string[] whereColumns, string tableName = "", string schemaName = "dbo") where T : class
        {
            if (dataObjects == null || dataObjects.Count == 0)
            {
                throw new ArgumentException("compositeModelBuilder must not be null or empty.", nameof(dataObjects));
            }
            if (whereColumns == null || whereColumns.Length == 0)
            {
                throw new ArgumentException("whereColumns must not be null or empty.", nameof(whereColumns));
            }
            TruncateSchemaName(ref schemaName);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            var deleteQuery = _queryBuilder.BuildDeleteQuery(typeof(T), whereColumns, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(typeof(T), tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns, true);

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    foreach (var dataObject in dataObjects)
                    {
                        using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                        {
                            oCmd.CommandText = deleteQuery;
                            oCmd.CommandTimeout = 0; //todo: make it configurable
                            oCmd.Transaction = scope.Transaction;
                            AddCommandParameter(oCmd, props, metaDatas, dataObject, whereColumns, true);
                            oCmd.ExecuteNonQuery();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during delete: {ex.Message}\nQuery: {deleteQuery}", ex);
            }

        }


        /// <summary>
        /// Updates the specified data object into the database table.
        /// </summary>
        /// <typeparam name="T">The type of the data object to update.</typeparam>
        /// <param name="dataObject">The data object to update. Must not be null.</param>
        /// <param name="insertColumns">The columns to include in the update operation. If null, all columns are included.</param>
        /// <param name="updateColumns">The columns to include in the update operation. If null, all columns are included.</param>
        /// <param name="whereColumns">The whereColumns to include in the update operation. If null, no columns are included.</param>
        /// <param name="tableName">The name of the table to update into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObject"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during update.</exception>
        public void InsertOrUpdate<T>(object dataObject, IEnumerable<string>? insertColumns, IEnumerable<string>? updateColumns, string[]? whereColumns, string tableName = "", string schemaName = "dbo") where T : class
        {
            if (dataObject == null)
            {
                throw new ArgumentException("compositeModelBuilder must not be null or empty.", nameof(dataObject));
            }

            List<T> dataObjects = dataObject as List<T>;
            if (dataObjects == null)
            {
                dataObjects = new List<T>();
                dataObjects.Add((T)dataObject);
            }
            TruncateSchemaName(ref schemaName);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            //var updateQuery = _queryBuilder.BuildUpdateQuery(dataObject.GetType(), columns!, whereColumns, tableName, schemaName);

            var insertQuery = _queryBuilder.BuildInsertQuery(typeof(T), insertColumns, tableName, schemaName, _databaseProvider == DatabaseProvider.Sqlite ? true : false);

            var updateQuery = _queryBuilder.BuildUpdateQuery(typeof(T), updateColumns, whereColumns, tableName, schemaName);

            var whereQuery = _queryBuilder.GetWhereClause(typeof(T), whereColumns, tableName, schemaName);

            var _tableName = _metaDataValidator.GetTableName(typeof(T), tableName, schemaName);

            string dynamicQuery;
            if (_databaseProvider == DatabaseProvider.Sqlite)
            {
                /*
                 * UPDATE Inventory  SET Quantity = 5  Where ItemId = 1   AND EXISTS (SELECT 1 FROM Inventory  Where ItemId = 1);
                    INSERT INTO Inventory (Id, ItemId, Quantity) 
                    select * from (select 1 'Id', 1 'ItemId', 1 'Quantity')  AS new_data
                     WHERE NOT EXISTS (SELECT 1 FROM Inventory  Where ItemId = 1);
                 * */
                dynamicQuery = string.Format(@"
                    {1} AND EXISTS (SELECT 1 FROM {0} {2});
                    {3}
                    WHERE NOT EXISTS (SELECT 1 FROM {0} {2});",
                    _tableName, updateQuery, whereQuery, insertQuery);
            }
            else
            {
                dynamicQuery = string.Format(@"
                    if exists(select 1 from {0} {1} )
                    begin
                     {2}
                    end
                    else
                    begin
                       {3}
                    end ", _tableName, whereQuery, updateQuery, insertQuery);
            }

            try
            {
                MetaDataInfo[] insertColumnsMetaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, insertColumns, _databaseProvider == DatabaseProvider.Sqlite ? true : false);
                MetaDataInfo[] updateColumnsmetaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, updateColumns, true);
                MetaDataInfo[] whereColumnsMetaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns, true);

                var metaDatasCommon = insertColumnsMetaDatas.Union(updateColumnsmetaDatas).ToArray();

                string[] columnsCommon = MergeCoumnsWhenNotNull(insertColumns, updateColumns);

                _transactionManager.ExecuteInTransaction(scope =>
                {
                    foreach (var data in dataObjects)
                    {
                        using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                        {
                            oCmd.CommandText = dynamicQuery;
                            oCmd.CommandTimeout = 0; //todo: make it configurable
                            oCmd.Transaction = scope.Transaction;
                            AddCommandParameter(oCmd, props, metaDatasCommon, data, columnsCommon);
                            AddCommandParameter(oCmd, props, whereColumnsMetaDatas, data, whereColumns, true);

                            oCmd.ExecuteNonQuery();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error during update: {ex.Message}\nQuery: {dynamicQuery}", ex);
            }

        }

        private static string[] MergeCoumnsWhenNotNull(IEnumerable<string>? insertColumns, IEnumerable<string> updateColumns)
        {
            var columnsCommon = new string[] { };
            if (updateColumns == null || insertColumns == null)
                return null;

            columnsCommon = columnsCommon.Union(updateColumns.ToArray()).ToArray();
            columnsCommon = columnsCommon.Union(insertColumns.ToArray()).ToArray();

            return columnsCommon;
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
            TruncateSchemaName(ref schemaName);

            var selectQuery = _queryBuilder.BuildSelectQuery(entityType, columns, whereClause, orderByColumn, limit, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(entityType, tableName, schemaName);

                return _transactionManagerSelect.ExecuteInTransaction(scope =>
                {
                    using (IDbCommand oCmd = _IDbConnectionSelect.CreateCommand())
                    {
                        oCmd.CommandText = selectQuery;
                        oCmd.CommandTimeout = 0; //todo: make it configurable
                        oCmd.Transaction = scope.Transaction;

                        IDbDataAdapter adapter = DBObjectFactory.GetDbDataAdapter(_databaseProvider);
                        adapter.SelectCommand = oCmd;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        scope.Commit();
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
            TruncateSchemaName(ref schemaName);

            var selectQuery = _queryBuilder.BuildSelectQuery(entityType, columns, whereColumns?.Select(m => m.Key).ToArray(), orderByColumn, limit, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(entityType, tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns?.Select(m => m.Key).ToArray(), true);

                return _transactionManagerSelect.ExecuteInTransaction(scope =>
                {
                    using (IDbCommand oCmd = _IDbConnectionSelect.CreateCommand())
                    {
                        oCmd.CommandText = selectQuery;
                        oCmd.CommandTimeout = 0; //todo: make it configurable
                        oCmd.Transaction = scope.Transaction;
                        AddCommandParameter(oCmd, props, metaDatas, whereColumns);

                        IDbDataAdapter adapter = DBObjectFactory.GetDbDataAdapter(_databaseProvider);
                        adapter.SelectCommand = oCmd;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        scope.Commit();
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
            TruncateSchemaName(ref schemaName);

            var selectQuery = _queryBuilder.BuildSelectQuery(entityType, columns, whereColumns?.Select(m => m.Key).ToArray(), orderByColumn, limit, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(entityType, tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, true);
                MetaDataInfo[] metaDatasWhere = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns?.Select(m => m.Key).ToArray(), true);

                // return _transactionManagerSelect.ExecuteInTransaction<T>(selectQuery, metaDatas);
                using (IDbCommand oCmd = _IDbConnectionSelect.CreateCommand())
                {
                    oCmd.CommandText = selectQuery;
                    oCmd.CommandTimeout = 0; //todo: make it configurable
                    AddCommandParameter(oCmd, props, metaDatasWhere, whereColumns);
                    return _transactionManagerSelect.ExecuteInTransaction<T>(oCmd, metaDatas);
                }
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
        /// <param name="whereClause">The whereClause to include in the select operation.</param>
        /// <param name="orderByColumn">The orderByColumn to include in the select operation.</param>
        /// <param name="limit">The limit to include in the select operation.</param>
        /// <param name="tableName">The name of the table to delete into. If empty, the table name is inferred from the type.</param>
        /// <param name="schemaName">The schema name of the table. Defaults to "dbo".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityType"/> is null.</exception>
        /// <exception cref="Exception">Thrown if a database error occurs during select.</exception>
        public IEnumerable<T> Select<T>(Type entityType, string[]? columns = null, string whereClause = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo") where T : class, new()
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(entityType);
            TruncateSchemaName(ref schemaName);

            var selectQuery = _queryBuilder.BuildSelectQuery(entityType, columns, whereClause, orderByColumn, limit, tableName, schemaName);

            try
            {
                var _tableName = _metaDataValidator.GetTableName(entityType, tableName, schemaName);
                MetaDataInfo[] metaDatas = _metaDataValidator.GetValidColumns(schemaName, _tableName, columns, true);
                //MetaDataInfo[] metaDatasWhere = _metaDataValidator.GetValidColumns(schemaName, _tableName, whereColumns?.Select(m => m.Key).ToArray(), true);

                //return _transactionManagerSelect.ExecuteInTransaction<T>(selectQuery, metaDatas);
                IDbCommand oCmd = _IDbConnectionSelect.CreateCommand();
                
                    oCmd.CommandText = selectQuery;
                    oCmd.CommandTimeout = 0; //todo: make it configurable
                    return _transactionManagerSelect.ExecuteInTransaction<T>(oCmd, metaDatas);
                
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
                string schemaName = c.SchemaName;   
                TruncateSchemaName(schemaName: ref schemaName);
                c.SchemaName = schemaName;

                string dynamicQuery = "";
                if (c.OperationMode == OperationMode.Update)
                {
                    var updateQuery = _queryBuilder.BuildUpdateQuery(c.ObjectType, c.UpdateColumns, c.WhereColumns, c.TableName, c.SchemaName);

                    c.GeneratedQuery = updateQuery;
                }
                else if (c.OperationMode == OperationMode.Delete)
                {
                    var deleteQuery = _queryBuilder.BuildDeleteQuery(c.ObjectType, c.WhereColumns, c.TableName, c.SchemaName);

                    c.GeneratedQuery = deleteQuery;
                }
                else if (c.OperationMode == OperationMode.Insert)
                {
                    var insertQuery = _queryBuilder.BuildInsertQuery(c.ObjectType, c.InsertColumns, c.TableName, c.SchemaName, _databaseProvider == DatabaseProvider.Sqlite ? true : false);

                    c.GeneratedQuery = insertQuery;
                }
                else if (c.OperationMode == OperationMode.InsertOrUpdate)
                {
                    var insertQuery = _queryBuilder.BuildInsertQuery(c.ObjectType, c.InsertColumns, c.TableName, c.SchemaName, _databaseProvider == DatabaseProvider.Sqlite ? true : false);

                    var updateQuery = _queryBuilder.BuildUpdateQuery(c.ObjectType, c.UpdateColumns, c.WhereColumns, c.TableName, c.SchemaName);

                    var whereQuery = _queryBuilder.GetWhereClause(c.ObjectType, c.WhereColumns, c.TableName, c.SchemaName);

                    var tableName = _queryBuilder.GetTableName(c.ObjectType, c.TableName, c.SchemaName);


                    if (_databaseProvider == DatabaseProvider.Sqlite)
                    {
                        /*
                         * UPDATE Inventory  SET Quantity = 5  Where ItemId = 1   AND EXISTS (SELECT 1 FROM Inventory  Where ItemId = 1);
                            INSERT INTO Inventory (Id, ItemId, Quantity) 
                            select * from (select 1 'Id', 1 'ItemId', 1 'Quantity')  AS new_data
                             WHERE NOT EXISTS (SELECT 1 FROM Inventory  Where ItemId = 1);
                         * */
                        dynamicQuery = string.Format(@"
                        {1} AND EXISTS (SELECT 1 FROM {0} {2});
                        {3}
                        WHERE NOT EXISTS (SELECT 1 FROM {0} {2});",
                                tableName, updateQuery, whereQuery, insertQuery);
                    }
                    else
                    {
                        dynamicQuery = string.Format(@"
                        if exists(select 1 from {0} {1} )
                        begin
                         {2}
                        end
                        else
                        begin
                           {3}
                        end ", tableName, whereQuery, updateQuery, insertQuery);
                    }

                    //dynamicQuery = string.Format(@"
                    //                    if exists(select 1 from {0}  {1} )
                    //                    begin
                    //                     {2}
                    //                    end
                    //                    else
                    //                    begin
                    //                       {3}
                    //                    end ", tableName, whereQuery, updateQuery, insertQuery);


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
                        string schemaName = c.SchemaName;
                        TruncateSchemaName(schemaName: ref schemaName);
                        c.SchemaName = schemaName;

                        if (c.OperationMode == OperationMode.Custom)
                        {

                            using (IDbCommand oCmd = _IDbConnection.CreateCommand())
                            {
                                oCmd.CommandText = c.GeneratedQuery;
                                currentQuery = c.GeneratedQuery;
                                oCmd.CommandTimeout = 0;
                                oCmd.Transaction = scope.Transaction;
                                oCmd.ExecuteNonQuery();
                            }

                            continue;
                        }

                        PropertyDescriptorCollection props = TypeDescriptor.GetProperties(c.ObjectType);

                        var _tableName = _metaDataValidator.GetTableName(c.ObjectType, c.TableName, c.SchemaName);
                        MetaDataInfo[] insertColumnsMetaDatas = _metaDataValidator.GetValidColumns(c.SchemaName, _tableName, c.InsertColumns, _databaseProvider == DatabaseProvider.Sqlite ? true : false);
                        MetaDataInfo[] updateColumnsmetaDatas = _metaDataValidator.GetValidColumns(c.SchemaName, _tableName, c.UpdateColumns, true);
                        MetaDataInfo[] whereColumnsmetaDatas = _metaDataValidator.GetValidColumns(c.SchemaName, _tableName, c.WhereColumns, true);

                        var metaDatasCommon = insertColumnsMetaDatas.Union(updateColumnsmetaDatas).ToArray();
                        string[] columnsCommon = MergeCoumnsWhenNotNull(c.InsertColumns, c.UpdateColumns);

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
                                    AddCommandParameter(oCmd, props, updateColumnsmetaDatas, dataObject, c.UpdateColumns, false);
                                    AddCommandParameter(oCmd, props, whereColumnsmetaDatas, dataObject, c.WhereColumns, true);
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
                                    AddCommandParameter(oCmd, props, whereColumnsmetaDatas, dataObject, c.WhereColumns, true);
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
                                    AddCommandParameter(oCmd, props, metaDatasCommon, dataObject, columnsCommon);
                                    AddCommandParameter(oCmd, props, whereColumnsmetaDatas, dataObject, c.WhereColumns, true);
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

        /// <summary>
        /// Commits the current write transaction managed by this client.
        /// </summary>
        public void CommitTransaction()
        {
            _transactionManager.Commit();
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
                parameter.ParameterName = "@W" + prop.Name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
        }

        private void AddCommandParameter(IDbCommand command, PropertyDescriptorCollection props, MetaDataInfo[] metaDatas, object dataObject, IEnumerable<string>? columns, bool isWhereColumns = false)
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
                parameter.ParameterName = isWhereColumns ? "@W" + prop.Name : "@" + prop.Name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
        }

        private void TruncateSchemaName(ref string schemaName)
        {
            if (!string.IsNullOrEmpty(schemaName) && _databaseProvider == DatabaseProvider.Sqlite)
            {
                schemaName = "";
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
