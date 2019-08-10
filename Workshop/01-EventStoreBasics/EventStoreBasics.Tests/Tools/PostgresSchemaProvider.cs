using System.Collections;
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
            @"select column_name as name, data_type as type
              from INFORMATION_SCHEMA.COLUMNS where table_name = '@tableName'";

        public PostgresSchemaProvider(NpgsqlConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public Table GetTable(string tableName)
        {
            var columns =  databaseConnection.Query<Column>(GetTableColumns);

            return columns.Any() ? null : new Table(tableName, columns);
        }
    }

    public class Table
    {
        public string Name { get; }
        public IEnumerable<Column> Columns { get; }

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
        public const string StringType = "varchar";
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
