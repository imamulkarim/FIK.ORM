using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIK.ORM.Infrastructures.MetaData
{
    internal interface IMetaDataProvider
    {
        bool IsValidTable(string schemaName,string tableName);
        IEnumerable<MetaDataInfo> GetTableMetaData(string schemaName,string tableName, out IEnumerable<MetaDataInfo> columns);
        void RetriveTableMetaData(string schemaName,string tableName);
    }
}
