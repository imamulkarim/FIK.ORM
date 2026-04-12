using FIK.ORM.Extensions;
using FIK.ORM.Infrastructures.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIK.ORM.Infrastructures.QueryBuilders
{
    internal class SqLiteQueryBuilder : MetaDataValidator, IQueryBuilder
    {
        //private readonly IDbConnection _dbConnection;
        //private readonly IDbDataAdapter _dbDataAdapter;
        private readonly IMetaDataProvider metaDataProvider;

        public SqLiteQueryBuilder(IMetaDataProvider metaDataProvider) : base(metaDataProvider)
        {
            this.metaDataProvider = metaDataProvider;
        }

        public string BuildCountQuery(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
        {
            schemaName = "";
            ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
            ValidateColumns(GetTableName(entityType, tableName, schemaName), whereColumns!, schemaName);
            var query = $"SELECT COUNT(1) FROM {GetTableName(entityType, tableName, schemaName)} {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)} ";
            return query;
        }

        public string BuildDeleteQuery(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
        {
            schemaName = "";
            ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
            ValidateColumns(GetTableName(entityType, tableName, schemaName), whereColumns!, schemaName);
            var query = $"DELETE FROM {GetTableName(entityType, tableName, schemaName)} {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)} ";
            return query;
        }


        public string BuildInsertQuery(Type entityType, IEnumerable<string>? columns, string tableName = "", string schemaName = "dbo", bool withIdentityColumn = false)
        {
            schemaName = "";
            ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
            ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
            var query = $"INSERT INTO {GetTableName(entityType, tableName, schemaName)} {GetInsertColumns(schemaName, GetTableName(entityType, tableName, schemaName), columns!, withIdentityColumn)} ";
            return query;
        }

        internal override string GetInsertColumns(string schemaName, string tableName, IEnumerable<string>? columns, bool withIdentityColumn = false)
        {
            metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);

           
            var validCoumns = columns?.Where(column =>
            !dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            validCoumns = validCoumns ?? dbColumns.Select(c => c.ColumnName).ToList();

            var columnListWithIdenty = string.Join(", ", validCoumns);
            var parameterListWithIdentity = string.Join(", ", validCoumns.Select(c => "@" + c));
            return $"({columnListWithIdenty}) SELECT {parameterListWithIdentity}";
            
        }

        public string BuildSelectQuery(Type entityType, string[]? columns = null, string? whereClause = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo")
        {
            schemaName = "";
            ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
            ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
            var query = $"SELECT {GetSelectColumns(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} FROM {GetTableName(entityType, tableName, schemaName)} {whereClause?.SanitizeForSql()}  ";
            return query;
        }

        public string BuildSelectQuery(Type entityType, string[]? columns = null, string[]? whereColumns = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo")
        {
            schemaName = "";
            ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
            ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
            var query = $"SELECT {GetSelectColumns(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} FROM {GetTableName(entityType, tableName, schemaName)}  {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)}  ";
            return query;
        }



        public string BuildUpdateQuery(Type entityType, IEnumerable<string> columns, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
        {
            schemaName = "";
            ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
            ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
            var query = $"UPDATE {GetTableName(entityType, tableName, schemaName)} {BuildUpdateSetClause(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)}  ";
            return query;
        }

        public string GetWhereClause(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
        {
            schemaName = "";
            var whereClause = BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!);
            return whereClause;
        }
    }

}