namespace FIK.ORM.Enums;

/// <summary>
/// Specifies the type of operation represented by a composite model entry.
/// </summary>
public enum OperationMode
{
    /// <summary>
    /// Inserts a new record.
    /// </summary>
    Insert,

    /// <summary>
    /// Updates an existing record.
    /// </summary>
    Update,

    /// <summary>
    /// Deletes an existing record.
    /// </summary>
    Delete,

    /// <summary>
    /// Inserts a new record or updates an existing one.
    /// </summary>
    InsertOrUpdate,

    /// <summary>
    /// Executes a custom raw SQL statement.
    /// </summary>
    Custom
}
