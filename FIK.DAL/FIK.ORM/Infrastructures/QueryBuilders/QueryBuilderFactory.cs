using FIK.ORM.Enums;
using FIK.ORM.Infrastructures.MetaData;
using System;

namespace FIK.ORM.Infrastructures.QueryBuilders
{
    internal class QueryBuilderFactory
    {
        public static IQueryBuilder GetQueryBuilder(DatabaseProvider databaseProvider, IMetaDataProvider metaDataProvider)
        {
            // Initialize the factory based on the database provider
            IQueryBuilder queryBuilder = null!;
            switch (databaseProvider)
            {
                case DatabaseProvider.SqlServer:
                    // Register SQL Server specific factories
#if NET40
                    queryBuilder = new SQLQueryBuilder(metaDataProvider);
#elif NET6_0_OR_GREATER
                    queryBuilder = new SQLQueryBuilder(metaDataProvider);
#endif
                    break;
                case DatabaseProvider.Sqlite:
#if NETSTANDARD2_0                  
                    queryBuilder = new SQLQueryBuilder(metaDataProvider); //todo: implement SQLiteQueryBuilder
#endif
                    break;
                case DatabaseProvider.PostgreSQL:
                    // Register SQL Server specific factories
#if NET6_0_OR_GREATER
                    queryBuilder = new SQLQueryBuilder(metaDataProvider); //todo: implement PostgreSQLQueryBuilder
#endif
                    break;
                // Add cases for other database providers as needed
                default:
                    throw new NotSupportedException($"Database provider {databaseProvider} is not supported.");
            }

            return queryBuilder;
        }
    }
}
