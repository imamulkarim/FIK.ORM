using FIK.ORM.Enums;
using System;
using System.Collections.Generic;


namespace FIK.ORM.Models
{
    /// <summary>
    /// Represents a single operation entry inside a composite execution batch.
    /// </summary>
    public class CompositeModel
    {
        internal List<object> Model { get; set; }
        internal OperationMode OperationMode { get;  set; }
        internal string[]? InsertColumns { get;  set; }
        internal string[] WhereColumns { get;  set; }
        internal string[] UpdateColumns { get; set; }
        internal Type ObjectType { get; set; }
        internal string TableName { get; set; }
        internal string SchemaName { get; set; }
        internal string RawQuery { get; set; }

        internal string GeneratedQuery { get; set; } 

    }

   
}
