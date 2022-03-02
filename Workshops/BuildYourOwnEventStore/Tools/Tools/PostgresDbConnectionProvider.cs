using System.Diagnostics;
using Dapper;
using Npgsql;

namespace Tools.Tools;

public static class PostgresDbConnectionProvider
{
    public static NpgsqlConnection GetFreshDbConnection()
    {
        // get the test class name that will be used as POSTGRES schema
        var testClassName = new StackTrace().GetFrame(1)!.GetMethod()!.DeclaringType!.Name;
        // each test will have it's own schema name to run have data isolation and not interfere other tests
        var connection = new NpgsqlConnection(Settings.ConnectionString + $"Search Path= '{testClassName}'");

        // recreate schema to have it fresh for tests. Kids do not try that on production.
        connection.Execute($"DROP SCHEMA IF EXISTS {testClassName} CASCADE; CREATE SCHEMA {testClassName};");

        return connection;
    }
}
