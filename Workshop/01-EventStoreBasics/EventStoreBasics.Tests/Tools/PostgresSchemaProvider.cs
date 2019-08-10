using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;

namespace EventStoreBasics.Tests.Tools
{
    public class PostgresSchemaProvider
    {
        private readonly NpgsqlConnection databaseConnection;

        const string GetTableColumns =
            @"SELECT column_name AS name, data_type AS type
              FROM INFORMATION_SCHEMA.COLUMNS
              WHERE
                  table_name = @tableName
                  AND table_schema in (select schemas[1] from (select current_schemas(false) as schemas) as currentschema)";

        public PostgresSchemaProvider(NpgsqlConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public Table GetTable(string tableName)
        {
            var columns =  databaseConnection.Query<Column>(GetTableColumns, new { tableName });

            return columns.Any() ? new Table(tableName, columns) : null;
        }
    }

    public class Table
    {
        public string Name { get; }
        private IEnumerable<Column> Columns { get; }

        public Table(string name, IEnumerable<Column> columns)
        {
            Name = name;
            Columns = columns;
        }

        public Column GetColumn(string columnName)
        {
            return Columns.SingleOrDefault(column => column.Name == columnName);
        }
    }

    public class Column
    {
        public const string GuidType = "uuid";
        public const string LongType = "bigint";
        public const string StringType = "text";
        public const string JSONType = "jsonb";

        public string Name { get; }
        public string Type { get; }

        public Column(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
