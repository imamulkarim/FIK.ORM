using FIK.ORM.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FIK.ORM.Infrastructures.MetaData
{
    internal class MetaDataProviderFactory
    {
        public static IMetaDataProvider GetMetaDataProvider(DatabaseProvider databaseProvider, IDbConnection dbConnection)
        {
            // Initialize the factory based on the database provider
            IMetaDataProvider metaDataProvider = null;
            switch (databaseProvider)
            {
                case DatabaseProvider.SqlServer:
                    // Register SQL Server specific factories
#if NET40
                    metaDataProvider = new MetaDataProviderSQL(dbConnection);
#elif NET6_0_OR_GREATER
                    metaDataProvider = new MetaDataProviderSQL(dbConnection);
#endif
                    break;
                case DatabaseProvider.Sqlite:
                    metaDataProvider = new MetaDataProviderSQLite(dbConnection);
                    break;
                case DatabaseProvider.PostgreSQL:
                    // Register SQL Server specific factories
#if NET6_0_OR_GREATER
                    metaDataProvider = new MetaDataProviderPostgreSQL(dbConnection); 
#endif
                    break;
                // Add cases for other database providers as needed
                default:
                    throw new NotSupportedException($"Database provider {databaseProvider} is not supported.");
            }

            return metaDataProvider;
        }
    }
}
