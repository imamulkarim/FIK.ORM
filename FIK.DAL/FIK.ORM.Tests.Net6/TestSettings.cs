using Microsoft.Extensions.Configuration;

namespace FIK.ORM.Tests.Net6;

public static class TestSettings
{
    private static IConfiguration _configuration;

    static TestSettings()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        _configuration = builder.Build();
    }

    public static string ConnectionString
    {
        get
        {
            var connectionString = _configuration.GetConnectionString("TestDatabase");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing 'TestDatabase' connection string in appsettings.json.");
            }

            return connectionString;
        }
    }

    public static string DatabaseProvider
    {
        get
        {
            var provider = _configuration["DatabaseProvider"];
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new InvalidOperationException("Missing 'DatabaseProvider' setting in appsettings.json.");
            }

            return provider;
        }
    }
}
