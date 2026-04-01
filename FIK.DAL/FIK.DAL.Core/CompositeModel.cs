using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIK.ORM
{
    public class CompositeModel
    {
        internal List<object> Model { get; set; }
        internal OperationMode OperationMode { get;  set; }
        internal string[]? InsertColumns { get;  set; }
        internal Dictionary<string, string> WhereColumns { get;  set; }
        internal string[] GetWhereColumns()
        {
            return WhereColumns.Keys.ToArray();
        }
        internal string[] UpdateColumns { get; set; }
        internal Type ObjectType { get; set; }
        internal string TableName { get; set; }
        internal string SchemaName { get; set; }
        internal string RawQuery { get; set; }

        internal string GeneratedQuery { get; set; } 

    }

    public enum OperationMode
    {
        Insert, Update, Delete, InsertOrUpdate, Custom
    }
}
