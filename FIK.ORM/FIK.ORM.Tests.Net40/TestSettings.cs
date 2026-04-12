using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace FIK.ORM.Tests.Net40
{
    public static class TestSettings
    {
        public static string ConnectionString
        {
            get
            {
                var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"];
                if (connectionString == null || string.IsNullOrWhiteSpace(connectionString.ConnectionString))
                {
                    throw new InvalidOperationException("Missing 'TestDatabase' connection string in App.config.");
                }

                return connectionString.ConnectionString;
            }
        }

        public static string ConnectionStringSqlite
        {
            get
            {
                var connectionString = ConfigurationManager.ConnectionStrings["TestDatabaseSqLite"];
                if (connectionString == null || string.IsNullOrWhiteSpace(connectionString.ConnectionString))
                {
                    throw new InvalidOperationException("Missing 'TestDatabase' connection string in App.config.");
                }

                return connectionString.ConnectionString;
            }
        }

        public static string DatabaseProvider
        {
            get
            {
                var provider = ConfigurationManager.AppSettings["DatabaseProvider"];
                if (string.IsNullOrWhiteSpace(provider))
                {
                    throw new InvalidOperationException("Missing 'DatabaseProvider' appSetting in App.config.");
                }

                return provider;
            }
        }
    }
}
