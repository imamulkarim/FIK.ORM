using FIK.ORM.Enums;
using FIK.ORM.Models;
using System;
using System.Collections.Generic;

namespace FIK.ORM
{
    public class CompositeModelBuilder
    {
        List<CompositeModel> list = new List<CompositeModel>();

        /// <summary>
        /// <typeparam name="T"> Class Name </typeparam>
        /// <param name="model"> List or Single Object which need to perform operation </param>
        /// <param name="insertColumns">Optional Parameter , specify the columns to be insert </param>
        /// <param name="tableName"> Optional Parameter, specify the tableName name when it defers from model name</param>
        /// <param name="schemaName"> Optional Parameter, specify the schema name </param>
        /// <returns> true or false </returns>
        /// </summary>
        public bool AddInsertRecordSet<T>(object model,
                                    string[]? insertColumns,
                                    string tableName = "",
                                    string schemaName = "dbo")
        {
            try
            {

                List<T>? ListTob = model as List<T>;
                if (ListTob == null)
                {
                    ListTob = new List<T>();

                    //try to parse single data model
                    T Tob = (T)model;
                    if (Tob == null)
                    {
                        throw new Exception("Invalid Object Type");
                    }
                    ListTob.Add(Tob);
                }

                List<object> NewModel = new List<object>();
                foreach (T a in ListTob)
                {
                    NewModel.Add(a);
                }

                list.Add(new CompositeModel { Model = NewModel, TableName = tableName, ObjectType = typeof(T), OperationMode = OperationMode.Insert, InsertColumns = insertColumns, SchemaName = schemaName });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// <typeparam name="T"> Class Name </typeparam>
        /// <param name="model"> List or Single Object which need to perform operation </param>
        /// <param name="whereColumns"> Optional Parameter,when need only some specific property to insert sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="updateColumns"> Optional Parameter,when need only some specific property to update sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="tableName"> Optional Parameter, specify the tableName name when it defers from model name</param>
        /// <param name="schemaName"> Optional Parameter, specify the schema name </param>
        /// <returns> true or false </returns>
        /// </summary>
        public bool AddUpdateRecordSet<T>(object model,
                                    string[] whereColumns,
                                    string[]? updateColumns,
                                    string tableName = "",
                                    string schemaName = "dbo")
        {
            try
            {
                if ((whereColumns == null || whereColumns.Length == 0))
                {
                    throw new ArgumentException("whereColumns must not be null or empty.", nameof(whereColumns));
                }

                List<T>? ListTob = model as List<T>;
                if (ListTob == null)
                {
                    ListTob = new List<T>();

                    //try to parse single data model
                    T Tob = (T)model;
                    if (Tob == null)
                    {
                        throw new Exception("Invalid Object Type");
                    }
                    ListTob.Add(Tob);
                }

                List<object> NewModel = new List<object>();
                foreach (T a in ListTob)
                {
                    NewModel.Add(a);
                }

                list.Add(new CompositeModel { Model = NewModel, TableName = tableName, ObjectType = typeof(T), OperationMode = OperationMode.Update, WhereColumns = whereColumns, UpdateColumns = updateColumns, SchemaName = schemaName });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// <typeparam name="T"> Class Name </typeparam>
        /// <param name="model"> List or Single Object which need to perform operation </param>
        /// <param name="insertColumns">Optional Parameter , specify the columns to be insert </param>
        /// <param name="whereColumns"> Optional Parameter,when need only some specific property to insert sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="updateColumns"> Optional Parameter,when need only some specific property to update sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="tableName"> Optional Parameter, specify the tableName name when it defers from model name</param>
        /// <param name="schemaName"> Optional Parameter, specify the schema name </param>
        /// <returns> true or false </returns>
        /// </summary>
        public bool AddInsertOrUpdateRecordSet<T>(object model,
                                    string[]? insertColumns,
                                    string[]? whereColumns,
                                    string[]? updateColumns,
                                    string tableName = "",
                                    string schemaName = "dbo")
        {
            try
            {
                if ((whereColumns == null || whereColumns.Length == 0))
                {
                    throw new ArgumentException("whereColumns must not be null or empty.", nameof(whereColumns));
                }
                if ((updateColumns == null || updateColumns.Length == 0))
                {
                    throw new ArgumentException("updateColumns must not be null or empty.", nameof(updateColumns));
                }

                List<T>? ListTob = model as List<T>;
                if (ListTob == null)
                {
                    ListTob = new List<T>();

                    //try to parse single data model
                    T Tob = (T)model;
                    if (Tob == null)
                    {
                        throw new Exception("Invalid Object Type");
                    }
                    ListTob.Add(Tob);
                }

                List<object> NewModel = new List<object>();
                foreach (T a in ListTob)
                {
                    NewModel.Add(a);
                }

                list.Add(new CompositeModel { Model = NewModel, TableName = tableName, ObjectType = typeof(T), OperationMode = OperationMode.InsertOrUpdate, WhereColumns = whereColumns, InsertColumns = insertColumns, UpdateColumns = updateColumns, SchemaName = schemaName });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// <typeparam name="T"> Class Name </typeparam>
        /// <param name="model"> List or Single Object which need to perform operation </param>
        /// <param name="whereColumns"> Optional Parameter,when need only some specific property to insert sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="tableName"> Optional Parameter, specify the tableName name when it defers from model name</param>
        /// <param name="schemaName"> Optional Parameter, specify the schema name </param>
        /// <returns> true or false </returns>
        /// </summary>
        public bool AddDeleteRecordSet<T>(object model,
                                    string[]? whereColumns,
                                    string tableName = "",
                                    string schemaName = "dbo")
        {
            try
            {
                if ((whereColumns == null || whereColumns.Length == 0))
                {
                    throw new ArgumentException("whereColumns must not be null or empty.", nameof(whereColumns));
                }

                List<T>? ListTob = model as List<T>;
                if (ListTob == null)
                {
                    ListTob = new List<T>();

                    //try to parse single data model
                    T Tob = (T)model;
                    if (Tob == null)
                    {
                        throw new Exception("Invalid Object Type");
                    }
                    ListTob.Add(Tob);
                }

                List<object> NewModel = new List<object>();
                foreach (T a in ListTob)
                {
                    NewModel.Add(a);
                }

                list.Add(new CompositeModel { Model = NewModel, TableName = tableName, ObjectType = typeof(T), OperationMode = OperationMode.Delete, WhereColumns = whereColumns, SchemaName = schemaName });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// <typeparam name="T"> Class Name </typeparam>
        /// <param name="model"> List or Single Object which need to perform operation </param>
        /// <param name="operationMode"> enum type OperationMode </param>
        /// <param name="insertColumns">Optional Parameter , specify the columns to be insert </param>
        /// <param name="whereColumns"> Optional Parameter,when need only some specific property to insert sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="updateColumns"> Optional Parameter,when need only some specific property to update sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="tableName"> Optional Parameter, specify the tableName name when it defers from model name</param>
        /// <param name="schemaName"> Optional Parameter, specify the schema name </param>
        /// <returns> true or false </returns>
        /// </summary>
        public bool AddRecordSet<T>(object model,
                                    OperationMode operationMode,
                                    string[]? insertColumns,
                                    string[] whereColumns,
                                    string[]? updateColumns,
                                    string tableName = "",
                                    string schemaName = "dbo")
        {
            try
            {
                if ((operationMode == OperationMode.InsertOrUpdate || operationMode == OperationMode.Update || operationMode == OperationMode.Delete)
                        && (whereColumns == null || whereColumns.Length == 0))
                {
                    throw new ArgumentException("whereColumns must not be null or empty.", nameof(whereColumns));
                }

                List<T>? ListTob = model as List<T>;
                if (ListTob == null)
                {
                    ListTob = new List<T>();

                    //try to parse single data model
                    T Tob = (T)model;
                    if (Tob == null)
                    {
                        throw new Exception("Invalid Object Type");
                    }
                    ListTob.Add(Tob);
                }

                List<object> NewModel = new List<object>();
                foreach (T a in ListTob)
                {
                    NewModel.Add(a);
                }

                list.Add(new CompositeModel { Model = NewModel, TableName = tableName, ObjectType = typeof(T), OperationMode = operationMode, WhereColumns = whereColumns, InsertColumns = insertColumns, UpdateColumns = updateColumns, SchemaName = schemaName });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddRecordSetRawQuery(string query)
        {
            list.Add(new CompositeModel { RawQuery = query , OperationMode= OperationMode.Custom });
            return true;
        }


        public List<CompositeModel> GetRecordSet()
        {
            return list;
        }
    }
}
