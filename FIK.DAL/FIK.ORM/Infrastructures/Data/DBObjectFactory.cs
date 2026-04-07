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
            IDbConnection DBConnection = null;
            switch (databaseProvider)
            {
                case DatabaseProvider.SqlServer:
                    // Register SQL Server specific factories
#if NET40
                    DBConnection = new System.Data.SqlClient.SqlConnection(dbConnection);
                    //System.Data.IDbCommand  sqlCommandFactory = new System.Data.SqlClient.SqlCommand();
                    //System.Data.IDbDataAdapter sqlAdapterFactory = new System.Data.SqlClient.SqlDataAdapter();
                    //System.Data.IDbTransaction sqlTransactionFactory = null!;

                    //DBConnectionProvider = DBConnectionProvider.Create(DatabaseProvider.SqlServer);
                    //DBConnectionProvider.RegisterFactory<System.Data.SqlClient.SqlConnection>(sqlConnectionFactory);
                    //DBConnectionProvider.RegisterFactory<System.Data.SqlClient.SqlCommand>(sqlCommandFactory);
                    //DBConnectionProvider.RegisterFactory<System.Data.SqlClient.SqlDataAdapter>(sqlAdapterFactory);
                    //DBConnectionProvider.RegisterFactory<System.Data.SqlClient.SqlTransaction>(sqlTransactionFactory);
#elif NET6_0_OR_GREATER
                    DBConnection = new Microsoft.Data.SqlClient.SqlConnection(dbConnection);
                    //System.Data.IDbCommand sqlCommandFactory = new Microsoft.Data.SqlClient.SqlCommand();
                    //System.Data.IDbDataAdapter sqlAdapterFactory = new Microsoft.Data.SqlClient.SqlDataAdapter();
                    //System.Data.IDbTransaction sqlTransactionFactory = null!;

                    //DBConnectionProvider = DBConnectionProvider.Create(DatabaseProvider.SqlServer);
                    //DBConnectionProvider.RegisterFactory<Microsoft.Data.SqlClient.SqlConnection>(sqlConnectionFactory);
                    //DBConnectionProvider.RegisterFactory<Microsoft.Data.SqlClient.SqlCommand>(sqlCommandFactory);
                    //DBConnectionProvider.RegisterFactory<Microsoft.Data.SqlClient.SqlDataAdapter>(sqlAdapterFactory);
                    //DBConnectionProvider.RegisterFactory<Microsoft.Data.SqlClient.SqlTransaction>(sqlTransactionFactory);
#endif
                    break;
                    case DatabaseProvider.Sqlite:
#if NETSTANDARD2_0                  
                    DBConnection = new System.Data.SQLite.SQLiteConnection(dbConnection);
                    //System.Data.IDbCommand sqlCommand = new System.Data.SQLite.SQLiteCommand();
                    //System.Data.IDbDataAdapter sqlAdapter = new System.Data.SQLite.SQLiteDataAdapter();
                    //System.Data.IDbTransaction sqlTransaction = null!;

                    //DBConnectionProvider = DBConnectionProvider.Create(DatabaseProvider.SqlServer);
                    //DBConnectionProvider.RegisterFactory<System.Data.SQLite.SQLiteConnection>(sqlConnection);
                    //DBConnectionProvider.RegisterFactory<System.Data.SQLite.SQLiteCommand>(sqlCommand);
                    //DBConnectionProvider.RegisterFactory<System.Data.SQLite.SQLiteDataAdapter>(sqlAdapter);
                    //DBConnectionProvider.RegisterFactory<System.Data.SQLite.SQLiteTransaction>(sqlTransaction);
#endif
                    break;
                case DatabaseProvider.PostgreSQL:
                    // Register SQL Server specific factories
#if NET6_0_OR_GREATER
                    DBConnection = new Npgsql.NpgsqlConnection(dbConnection);;
                    //System.Data.IDbCommand postgreSQLCommand = new Npgsql.NpgsqlCommand();
                    //System.Data.IDbDataAdapter postgreSQLAdapter = new Npgsql.NpgsqlDataAdapter();
                    //System.Data.IDbTransaction postgreSQLTransaction = null!;

                    //DBConnectionProvider = DBConnectionProvider.Create(DatabaseProvider.PostgreSQL);
                    //DBConnectionProvider.RegisterFactory<Npgsql.NpgsqlConnection>(postgreSQLConnection);
                    //DBConnectionProvider.RegisterFactory<Npgsql.NpgsqlCommand>(postgreSQLCommand);
                    //DBConnectionProvider.RegisterFactory<Npgsql.NpgsqlDataAdapter>(postgreSQLAdapter);
                    //DBConnectionProvider.RegisterFactory<Npgsql.NpgsqlTransaction>(postgreSQLTransaction);
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
                    dbDataAdapter = new Microsoft.Data.SqlClient.SqlDataAdapter();
#endif
                    break;
                case DatabaseProvider.Sqlite:
#if NETSTANDARD2_0                  
                    dbDataAdapter = new System.Data.SQLite.SQLiteDataAdapter();
#endif
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



        //var command = provider.GetCommand<SqlCommand>();
        //        var connection = provider.GetConnection<SqlCommand>();
    }
}
