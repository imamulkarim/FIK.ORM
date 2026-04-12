namespace FIK.ORM.Infrastructures.MetaData;


#if NET6_0_OR_GREATER

/// <summary>
/// Represents metadata information for a database column.
/// </summary>
/// <param name="ColumnName">The name of the column.</param>
/// <param name="OrdinalPosition">The ordinal position of the column in the table.</param>
/// <param name="ColumnDefault">The default value of the column.</param>
/// <param name="IsNullable">Indicates whether the column allows null values.</param>
/// <param name="DataType">The data type of the column.</param>
/// <param name="CharacterMaximumLength">The maximum length for character columns.</param>
/// <param name="NumericPrecision">The precision for numeric columns.</param>
/// <param name="NumericPrecisionRadix">The precision radix for numeric columns.</param>
/// <param name="NumericScale">The scale for numeric columns.</param>
/// <param name="IdentityColumn">Gets the Identity info for column.</param>
public record MetaDataInfo(
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    string ColumnName,
    /// <summary>
    /// Gets the ordinal position of the column in the table.
    /// </summary>
    int OrdinalPosition,
    /// <summary>
    /// Gets the default value of the column.
    /// </summary>
    string ColumnDefault,
    /// <summary>
    /// Gets a value indicating whether the column allows null values.
    /// </summary>
    string IsNullable,
    /// <summary>
    /// Gets the data type of the column.
    /// </summary>
    string DataType,
    /// <summary>
    /// Gets the maximum length for character columns.
    /// </summary>
    int? CharacterMaximumLength,
    /// <summary>
    /// Gets the precision for numeric columns.
    /// </summary>
    int? NumericPrecision,
    /// <summary>
    /// Gets the precision radix for numeric columns.
    /// </summary>
    int? NumericPrecisionRadix,
    /// <summary>
    /// Gets the scale for numeric columns.
    /// </summary>
    int? NumericScale,
    /// <summary>
    /// Gets the Identity info for column.
    /// </summary>
    bool IdentityColumn
);
#else
/// <summary>
/// Represents metadata information for a database column.
/// </summary>
public class MetaDataInfo
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public MetaDataInfo(string columnName, int ordinalPosition, string columnDefault, string isNullable, string dataType, int? characterMaximumLength, int? numericPrecision, int? numericPrecisionRadix, int? numericScale,bool identityColumn)
    {
        ColumnName = columnName;
        OrdinalPosition = ordinalPosition;
        ColumnDefault = columnDefault;
        IsNullable = isNullable;
        DataType = dataType;
        CharacterMaximumLength = characterMaximumLength;
        NumericPrecision = numericPrecision;
        NumericPrecisionRadix = numericPrecisionRadix;
        NumericScale = numericScale;
        IdentityColumn=identityColumn;
    }

    public string ColumnName { get; private set; }
    public int OrdinalPosition { get; private set; }
    public string? ColumnDefault { get; private set; }
    public string IsNullable { get; private set; }
    public string DataType { get; private set; }
    public int? CharacterMaximumLength { get; private set; }
    public int? NumericPrecision { get; private set; }
    public int? NumericPrecisionRadix { get; private set; }
    public int? NumericScale { get; private set; }
    public bool IdentityColumn { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

}
#endif
