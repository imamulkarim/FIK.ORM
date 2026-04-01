using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIK.DAL
{
    public class CompositeModel
    {
        public List<object> Model { get; private set; }
        public OperationMode OperationMode { get; private set; }
        public string ExlcudeAutogeneratePrimaryKey { get; private set; }
        public string SlectiveProperty { get; private set; }
        public string SlectivePropertyUpdate { get;  set; }
        public string WhereClauseParamForUpdateDelete { get; private set; }
        public Type ObjectType { get; private set; }
        public string ObjectName { get; private set; }

        List<CompositeModel> list = new List<CompositeModel>();

        /// <summary>
        ///  wheen need to permorm isnert /update / delete on multiple table with same transaction
        /// </summary>
        /// <typeparam name="T"> Class Name </typeparam>
        /// <param name="model"> List or Single Object which need to perform operation </param>
        /// <param name="operationMode"> enum type OperationMode </param>
        /// <param name="exlcudeAutogeneratePrimaryKey">Optional Parameter , for skip auto generated column insert</param>
        /// <param name="slectiveProperty"> Optional when need only some specific property to insert sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="whereClauseParamForUpdate"> Optional generate And operation based where simple clause sample ( Id,Id2) </param>
        ///  <param name="customeTable"> Optional when table name not represent class name </param>
        /// <returns> true or false </returns>
        public bool AddRecordSet<T>(object model, OperationMode operationMode, string exlcudeAutogeneratePrimaryKey, string slectiveProperty, string whereClauseParamForUpdate, string customeTable)
        {
            try
            {

                List<T> ListTob = model as List<T>;
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

                string tableName = ListTob[0].GetType().Name;
                if (!string.IsNullOrEmpty(customeTable))
                    tableName = customeTable;

                list.Add(new CompositeModel { Model = NewModel, ObjectName = tableName, ObjectType = typeof(T), OperationMode = operationMode, ExlcudeAutogeneratePrimaryKey = exlcudeAutogeneratePrimaryKey, SlectiveProperty = slectiveProperty, WhereClauseParamForUpdateDelete = whereClauseParamForUpdate });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        ///  wheen need to permorm isnert /update / delete on multiple table with same transaction
        /// </summary>
        /// <typeparam name="T"> Class Name </typeparam>
        /// <param name="model"> List or Single Object which need to perform operation </param>
        /// <param name="operationMode"> enum type OperationMode </param>
        /// <param name="exlcudeAutogeneratePrimaryKey">Optional Parameter , for skip auto generated column insert</param>
        /// <param name="slectivePropertyInsert"> Optional when need only some specific property to insert sample ( Id,Name,Amount, +Qty ) ,  </param>
        /// <param name="slectivePropertyUpdate"> Optional when need only some specific property to insert sample ( Id,Name,Amount, +Qty ) , for update if existing data need to increment or decrement then use + or - </param>
        /// <param name="whereClauseParamForUpdate"> Optional generate And operation based where simple clause sample ( Id,Id2) </param>
        ///  <param name="customeTable"> Optional when table name not represent class name </param>
        /// <returns> true or false </returns>
        public bool AddRecordSet<T>(object model, OperationMode operationMode, string exlcudeAutogeneratePrimaryKey, string slectivePropertyInsert, string slectivePropertyUpdate, string whereClauseParamForUpdate, string customeTable)
        {
            try
            {

                List<T> ListTob = model as List<T>;
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

                string tableName = ListTob[0].GetType().Name;
                if (!string.IsNullOrEmpty(customeTable))
                    tableName = customeTable;

                list.Add(new CompositeModel { Model = NewModel, ObjectName = tableName, ObjectType = typeof(T), OperationMode = operationMode, ExlcudeAutogeneratePrimaryKey = exlcudeAutogeneratePrimaryKey, SlectiveProperty = slectivePropertyInsert, SlectivePropertyUpdate= slectivePropertyUpdate, WhereClauseParamForUpdateDelete = whereClauseParamForUpdate });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public List<CompositeModel> GetRecordSet()
        {
            return list;
        }

    }

    public enum OperationMode
    {
        Insert, Update, Delete, InsertOrUpdaet 
    }
}
