using FIK.ORM.Extensions;
using FIK.ORM.Infrastructures.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIK.ORM.Infrastructures.QueryBuilders;

internal class PostgreSQLQueryBuilder : MetaDataValidator, IQueryBuilder
{
    //private readonly IDbConnection _dbConnection;
    //private readonly IDbDataAdapter _dbDataAdapter;
    private readonly IMetaDataProvider metaDataProvider;

    public PostgreSQLQueryBuilder(IMetaDataProvider metaDataProvider) : base(metaDataProvider)
    {
        this.metaDataProvider = metaDataProvider;
    }

    public override string GetTableName(Type entityType, string? tableName, string schemaName = "dbo")
    {
        if (string.IsNullOrEmpty(schemaName))
            return string.Concat("\"", string.IsNullOrEmpty(tableName) == true ? entityType.Name : tableName!, "\"");
        else return $"\"{schemaName}\"." + string.Concat("\"", string.IsNullOrEmpty(tableName) == true ? entityType.Name : tableName!, "\"");
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
        ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
        var query = $"INSERT INTO {GetTableName(entityType, tableName, schemaName)} {GetInsertColumns(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} ";
        return query;
    }

    internal override string GetInsertColumns(string schemaName, string tableName, IEnumerable<string>? columns, bool withIdentityColumn = false)
    {
        metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);

        var nonIdentityColumns = columns?.Where(column =>
            !dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase) && c.IdentityColumn == true)
        ).ToList();

        nonIdentityColumns = nonIdentityColumns ?? dbColumns.Where(c => c.IdentityColumn == false).Select(c => c.ColumnName).ToList();



        var columnList = string.Join(", ", nonIdentityColumns.Select(s => $"\"{s}\""));
        var parameterList = string.Join(", ", nonIdentityColumns.Select(c => "@" + c));
        return $"({columnList}) VALUES ({parameterList})";
    }

    public override MetaDataInfo[] GetValidColumns(string schemaName, string tableName, IEnumerable<string>? columns, bool withIdentityColumn)
    {
        string _tbleName = string.Concat("\"", schemaName, "\".", "\"", tableName, "\"");
        metaDataProvider!.GetTableMetaData(schemaName, _tbleName, out IEnumerable<MetaDataInfo> dbColumns);

        var validColumns = columns is not null
            ? dbColumns.Where(column =>
                columns.Any(c => c.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase))
            ).ToList()
            : dbColumns.ToList();

        if (!withIdentityColumn)
        {
            validColumns = validColumns.Where(c => !c.IdentityColumn).ToList();
        }

        return validColumns.ToArray();
    }

    internal override string BuildWhereClause(string tableName, string[] columns)
    {
        if (columns == null || !columns.Any())
        {
            return "";
        }
        var whereConditions = columns.Select(column => $"\"{column}\" = @W{column}");
        return " Where " + string.Join(" AND ", whereConditions);
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

    internal override string GetSelectColumns(string schemaName, string tableName, IEnumerable<string>? columns)
    {
        metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);

        var validColumns = columns?.Where(column =>
            !dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        validColumns = validColumns ?? dbColumns.Select(c => c.ColumnName).ToList();

        var columnList = string.Join(", ", validColumns.Select(s => $"\"{s}\""));

        return columnList;
    }

    public string BuildUpdateQuery(Type entityType, IEnumerable<string> columns, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
    {
        ValidateTableName(GetTableName(entityType, tableName, schemaName), schemaName);
        ValidateColumns(GetTableName(entityType, tableName, schemaName), columns!, schemaName);
        var query = $"UPDATE {GetTableName(entityType, tableName, schemaName)} {BuildUpdateSetClause(schemaName, GetTableName(entityType, tableName, schemaName), columns!)} {BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!)}  ";
        return query;
    }

    override internal string BuildUpdateSetClause(string schemaName, string tableName, IEnumerable<string>? columns)
    {
        Dictionary<string, string> columnWithModifier = ExtractColumnModifier(columns);

        metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);
        var validColumns = (columnWithModifier == null ? columns : columnWithModifier.Select(m => m.Key).ToArray())?.Where(column =>
            dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase) && c.IdentityColumn == false)
        ).ToList();
        validColumns = validColumns ?? dbColumns.Where(c => c.IdentityColumn == false).Select(c => c.ColumnName).ToList();


        var setClause = "";
        if (columnWithModifier == null)
            setClause = string.Join(", ", validColumns.Select(c => $"\"{c}\" = @{c}"));
        else
            setClause = string.Join(", ", validColumns.Select(c => $"\"{c}\" = \"{c}\" {columnWithModifier[c]} @{c}"));

        return " SET " + setClause;
    }

    public string GetWhereClause(Type entityType, string[]? whereColumns = null, string tableName = "", string schemaName = "dbo")
    {
        var whereClause = BuildWhereClause(GetTableName(entityType, tableName, schemaName), whereColumns!);
        return whereClause;
    }
}