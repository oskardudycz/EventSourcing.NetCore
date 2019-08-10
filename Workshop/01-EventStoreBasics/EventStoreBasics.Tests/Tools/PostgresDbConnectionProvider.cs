using Dapper;
using Npgsql;

namespace EventStoreBasics.Tests.Tools
{
    public static class PostgresDbConnectionProvider
    {
        public static NpgsqlConnection GetFreshDbConnection()
        {
            var connection = new NpgsqlConnection(Settings.ConnectionString);

            // recreate schema to have it fresh for tests. Kids do not try that on production.
            connection.Execute("DROP SCHEMA public CASCADE; CREATE SCHEMA public;");

            return connection;
        }
    }
}
