using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIK.ORM.Infrastructures.MetaData
{
    internal class MetaDataValidator
    {

        private readonly IMetaDataProvider? metaDataProvider;

        public MetaDataValidator(IMetaDataProvider metaDataProvider)
        {
            this.metaDataProvider = metaDataProvider;
        }


        public void ValidateTableName(string tableName, string schemaName = "dbo")
        {
            if (!metaDataProvider!.IsValidTable(schemaName, tableName))
            {
                throw new ArgumentException($"The table name '{tableName}' is not valid.");
            }
        }

        public void ValidateColumns(string tableName, IEnumerable<string>? columns, string schemaName = "dbo")
        {
            if (columns == null || !columns.Any())
            {
                return;
            }

            Dictionary<string, string> columnWithModifier = ExtractColumnModifier(columns);


            metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> validColumns);

            foreach (var column in columnWithModifier == null ? columns : columnWithModifier.Select(m => m.Key).ToArray()!)
            {
                if (!validColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"The column name '{column}' is not valid for table '{tableName}'.");
                }
            }
        }

        public virtual MetaDataInfo[] GetValidColumns(string schemaName, string tableName, IEnumerable<string>? columns, bool withIdentityColumn)
        {
            metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);

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

        public virtual string GetTableName(Type entityType, string? tableName, string schemaName = "dbo")
        {
            if(string.IsNullOrEmpty(schemaName))
                return string.IsNullOrEmpty(tableName) == true ? entityType.Name : tableName!;
            else return $"{schemaName}." + (string.IsNullOrEmpty(tableName) == true ? entityType.Name : tableName!);
        }

        internal virtual string BuildWhereClause(string tableName, string[] columns)
        {
            if (columns == null || !columns.Any())
            {
                return "";
            }
            var whereConditions = columns.Select(column => $"{column} = @W{column}");
            return " Where " + string.Join(" AND ", whereConditions);
        }

        internal virtual string GetInsertColumns(string schemaName, string tableName, IEnumerable<string>? columns, bool withIdentityColumn = false)
        {
            metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);

            if (withIdentityColumn)
            {
                var validCoumns = columns?.Where(column =>
                !dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                validCoumns = validCoumns ?? dbColumns.Select(c => c.ColumnName).ToList();

                var columnListWithIdenty = string.Join(", ", validCoumns);
                var parameterListWithIdentity = string.Join(", ", validCoumns.Select(c => "@" + c));
                return $"({columnListWithIdenty}) VALUES ({parameterListWithIdentity})";
            }

            var nonIdentityColumns = columns?.Where(column =>
                !dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase) && c.IdentityColumn == true  )
            ).ToList();

            nonIdentityColumns = nonIdentityColumns ?? dbColumns.Where(c => c.IdentityColumn == false).Select(c => c.ColumnName).ToList();

            var columnList = string.Join(", ", nonIdentityColumns);
            var parameterList = string.Join(", ", nonIdentityColumns.Select(c => "@" + c));
            return $"({columnList}) VALUES ({parameterList})";
        }

        internal virtual string GetSelectColumns(string schemaName, string tableName, IEnumerable<string>? columns)
        {
            metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);

            var validColumns = columns?.Where(column =>
                !dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            validColumns = validColumns ?? dbColumns.Select(c => c.ColumnName).ToList();

            var columnList = string.Join(", ", validColumns);

            return columnList;
        }

        internal virtual string BuildUpdateSetClause(string schemaName, string tableName, IEnumerable<string>? columns)
        {
            Dictionary<string, string> columnWithModifier = ExtractColumnModifier(columns);

            metaDataProvider!.GetTableMetaData(schemaName, tableName, out IEnumerable<MetaDataInfo> dbColumns);
            var validColumns = (columnWithModifier == null ? columns : columnWithModifier.Select(m => m.Key).ToArray())?.Where(column =>
                dbColumns.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase) && c.IdentityColumn == false)
            ).ToList();
            validColumns = validColumns ?? dbColumns.Where(c=> c.IdentityColumn ==false).Select(c => c.ColumnName).ToList();


            var setClause = "";
            if(columnWithModifier == null)
                setClause= string.Join(", ", validColumns.Select(c => $"{c} = @{c}"));
            else            
                setClause = string.Join(", ", validColumns.Select(c => $"{c} = {c} {columnWithModifier[c]} @{c}"));

            return " SET " + setClause;
        }

        internal Dictionary<string, string> ExtractColumnModifier(IEnumerable<string>? columns)
        {
            if(columns == null || !columns.Any())
            {
                return null!;
            }
            Dictionary<string, string> columnWithModifier = new Dictionary<string, string>();
            foreach (var column in columns)
            {
                if (column.Contains("+") || column.Contains("-") || column.Contains("*") || column.Contains("/")) 
                    columnWithModifier.Add(column.Substring(1, column.Length-1 ) , column.Substring(0, 1));
            }
            return columnWithModifier.Count() == 0 ? null! : columnWithModifier;
        }

        private Dictionary<string, string> BuildOrderByClause(Dictionary<string, string>? orderByColumns)
        {
            if (orderByColumns == null || !orderByColumns.Any())
            {
                return new Dictionary<string, string>();
            }
            var validOrderByColumns = new Dictionary<string, string>();
            foreach (var kvp in orderByColumns)
            {
                var columnName = kvp.Key;
                var sortDirection = kvp.Value.ToUpper();
                if (sortDirection != "ASC" && sortDirection != "DESC")
                {
                    throw new ArgumentException($"Invalid sort direction '{kvp.Value}' for column '{columnName}'. Use 'ASC' or 'DESC'.");
                }
                validOrderByColumns[columnName] = sortDirection;
            }
            return validOrderByColumns;
        }


    }
}
