using Dapper;
using Npgsql;

namespace Tools.Tools;

/// <summary>
/// Helper class that returns information about the database schema (Tables and its Columns)
/// </summary>
public class PostgresSchemaProvider(NpgsqlConnection databaseConnection)
{
    const string GetTableColumnsSql =
        @"SELECT column_name AS name, data_type AS type
              FROM INFORMATION_SCHEMA.COLUMNS
              WHERE
                  table_name = @tableName
                  -- get only tables from current schema named as current test class
                  AND table_schema in (select schemas[1] from (select current_schemas(false) as schemas) as currentschema)";

    private const string FunctionExistsSql =
        "select exists(select * from pg_proc where proname = @functionName);";

    /// <summary>
    /// Returns schema information about specific table
    /// </summary>
    /// <param name="tableName">table name</param>
    /// <returns></returns>
    public Table? GetTable(string tableName)
    {
        var columns =  databaseConnection.Query<Column>(GetTableColumnsSql, new { tableName }).ToList();

        return columns.Any() ? new Table(tableName, columns) : null;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="functionName">function name</param>
    /// <returns></returns>
    public bool FunctionExists(string functionName) =>
        databaseConnection.QuerySingle<bool>(FunctionExistsSql, new {functionName});
}

/// <summary>
/// Describes basic information about database table
/// </summary>
public class Table(string name, IEnumerable<Column> columns)
{
    /// <summary>
    /// Table Name
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Table Columns
    /// </summary>
    private IEnumerable<Column> Columns { get; } = columns;

    public Column? GetColumn(string columnName) =>
        Columns.SingleOrDefault(column => column.Name == columnName);
}

/// <summary>
/// Describes basic information about database column
/// </summary>
public class Column(string name, string type)
{
    public const string GuidType = "uuid";
    public const string LongType = "bigint";
    public const string StringType = "text";
    public const string JsonType = "jsonb";
    public const string DateTimeType = "timestamp with time zone";

    /// <summary>
    /// Column Name
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Column Type
    /// </summary>
    public string Type { get; } = type;
}
