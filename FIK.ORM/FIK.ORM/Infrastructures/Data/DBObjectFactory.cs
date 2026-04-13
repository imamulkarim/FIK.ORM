using FIK.ORM.Enums;
using System;
using System.Data;


namespace FIK.ORM.Infrastructures.Data
{
    /// <summary>
    /// Provides a factory for creating and registering database connection-related factories
    /// based on the specified <see cref="DatabaseProvider"/>.
    /// </summary>
    internal class DBObjectFactory
    {
        /// <summary>
        /// Gets the <see cref="DBConnectionProvider"/> instance used by this factory.
        /// </summary>
        //public DBConnectionProvider DBConnectionProvider { get; private set; }
        //public readonly IDbConnection DBConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBObjectFactory"/> class with the specified
        /// database provider and connection string.
        /// </summary>
        /// <param name="databaseProvider">The type of database provider.</param>
        /// <param name="dbConnection">The database connection string.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown when the specified <paramref name="databaseProvider"/> is not supported.
        /// </exception>
        public static IDbConnection GetDbConnection(DatabaseProvider databaseProvider, string dbConnection)
        {
            // Initialize the factory based on the database provider
            IDbConnection DBConnection = null!;
            switch (databaseProvider)
            {
                case DatabaseProvider.SqlServer:
                    // Register SQL Server specific factories
#if NET40
                    DBConnection = new System.Data.SqlClient.SqlConnection(dbConnection);
#elif NET6_0_OR_GREATER
                    DBConnection = new System.Data.SqlClient.SqlConnection(dbConnection);
#endif
                    break;
                    case DatabaseProvider.Sqlite:
                    DBConnection = new System.Data.SQLite.SQLiteConnection(dbConnection);
                    break;
                case DatabaseProvider.PostgreSQL:
                    // Register SQL Server specific factories
#if NET6_0_OR_GREATER
                    DBConnection = new Npgsql.NpgsqlConnection(dbConnection);;
#endif
                    break;
                // Add cases for other database providers as needed
                default:
                    throw new NotSupportedException($"Database provider {databaseProvider} is not supported.");
            }

            return DBConnection;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DBObjectFactory"/> class with the specified
        /// database provider.
        /// </summary>
        /// <param name="databaseProvider">The type of database provider.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown when the specified <paramref name="databaseProvider"/> is not supported.
        /// </exception>
        public static IDbDataAdapter GetDbDataAdapter(DatabaseProvider databaseProvider)
        {
            // Initialize the factory based on the database provider
            IDbDataAdapter dbDataAdapter = null!;
            switch (databaseProvider)
            {
                case DatabaseProvider.SqlServer:
                    // Register SQL Server specific factories
#if NET40
                    dbDataAdapter = new System.Data.SqlClient.SqlDataAdapter();
#elif NET6_0_OR_GREATER
                    dbDataAdapter = new System.Data.SqlClient.SqlDataAdapter();
#endif
                    break;
                case DatabaseProvider.Sqlite:
                    dbDataAdapter = new System.Data.SQLite.SQLiteDataAdapter();
                    break;
                case DatabaseProvider.PostgreSQL:
                    // Register SQL Server specific factories
#if NET6_0_OR_GREATER
                    dbDataAdapter = new Npgsql.NpgsqlDataAdapter();
#endif
                    break;
                // Add cases for other database providers as needed
                default:
                    throw new NotSupportedException($"Database provider {databaseProvider} is not supported.");
            }
            return dbDataAdapter;
        }

    }
}
