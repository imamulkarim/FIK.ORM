using System;
using System.Collections.Generic;

namespace FIK.ORM.Infrastructures.QueryBuilders
{
    internal interface IQueryBuilder
    {
        string GetTableName(Type entityType, string? tableName, string schemaName = "dbo");

        string BuildCountQuery(Type entityType, string[]? whereColumns = null, string tableName="", string schemaName="dbo");
        string BuildSelectQuery(Type entityType, string[]? columns = null, string? whereClause = null, Dictionary<string,string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName="dbo");
        string BuildSelectQuery(Type entityType, string[]? columns = null, string[]? whereColumns = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo");
        string BuildInsertQuery(Type entityType, IEnumerable<string>? columns, string tableName = "", string schemaName="dbo", bool withIdentityColumn =false);
        string BuildUpdateQuery(Type entityType, IEnumerable<string> columns, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo");
        string BuildDeleteQuery(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo");

        string GetWhereClause(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo");


    }
}
