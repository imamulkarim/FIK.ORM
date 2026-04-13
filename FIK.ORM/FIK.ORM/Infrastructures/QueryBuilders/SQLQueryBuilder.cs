using FIK.ORM.Extensions;
using FIK.ORM.Infrastructures.MetaData;
using System;
using System.Collections.Generic;


namespace FIK.ORM.Infrastructures.QueryBuilders;

internal class SQLQueryBuilder : MetaDataValidator, IQueryBuilder
{
    //private readonly IDbConnection _dbConnection;
    //private readonly IDbDataAdapter _dbDataAdapter;
    private readonly IMetaDataProvider metaDataProvider;

    public SQLQueryBuilder(IMetaDataProvider metaDataProvider) : base(metaDataProvider)
    {
    }

    public string BuildCountQuery(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
    {
        ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
        ValidateColumns(GetTableName(entityType, tableName, schemaName), whereColumns!, schemaName);
        var query = $"SELECT COUNT(1) FROM {GetTableName(entityType, tableName, schemaName)} {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)} ";
        return query;
    }

    public string BuildDeleteQuery(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
    {
        ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
        ValidateColumns(GetTableName(entityType, tableName, schemaName), whereColumns!, schemaName);
        var query = $"DELETE FROM {GetTableName(entityType, tableName, schemaName)} {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)} ";
        return query;
    }

 
    public string BuildInsertQuery(Type entityType, IEnumerable<string>? columns, string tableName = "", string schemaName = "dbo", bool withIdentityColumn = false)
    {
        ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
        ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!,schemaName);
        var query = $"INSERT INTO {GetTableName(entityType, tableName, schemaName)} {GetInsertColumns(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} ";
        return query;
    }



    public string BuildSelectQuery(Type entityType, string[]? columns = null, string? whereClause = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo")
    {
        ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
        ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
        var query = $"SELECT {GetSelectColumns(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} FROM {GetTableName(entityType, tableName, schemaName)} {whereClause?.SanitizeForSql()}  ";
        return query;
    }

    public string BuildSelectQuery(Type entityType, string[]? columns = null, string[]? whereColumns = null, Dictionary<string, string>? orderByColumn = null, int? limit = null, string tableName = "", string schemaName = "dbo")
    {
        ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
        ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
        var query = $"SELECT {GetSelectColumns(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} FROM {GetTableName(entityType, tableName, schemaName)}  {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)}  ";
        return query;
    }

    

    public string BuildUpdateQuery(Type entityType, IEnumerable<string> columns, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
    {
        ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
        ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
        var query = $"UPDATE {GetTableName(entityType, tableName, schemaName)} {BuildUpdateSetClause(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)}  ";
        return query;
    }

    public string GetWhereClause(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
    {
        var whereClause = BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!);
        return whereClause;
    }
}


//// Usage
//var sqlFactory = new SqlServerObjectFactory("your-connection-string");
//    var provider = DBConnectionProvider.Create(DatabaseProvider.SqlServer);
//provider.RegisterFactory(sqlFactory);

//var command = provider.GetCommand<SqlCommand>();
//var connection = provider.GetConnection<SqlCommand>();