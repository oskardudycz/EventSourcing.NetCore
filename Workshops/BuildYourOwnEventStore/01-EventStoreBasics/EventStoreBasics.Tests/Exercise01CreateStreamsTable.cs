using System;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreBasics.Tests
{
    /// <summary>
    /// Exercise 01 - Create Streams Table
    /// </summary>
    /// <remarks>
    /// <para>
    /// Stream represents ordered set of events. Simply speaking stream is an log of the events that happened for the
    /// specific aggregate/entity.
    /// </para>
    ///
    /// <para>
    /// As truth is in the log, so in the events - stream can be also interpreted as the grouping, where key is stream id.
    /// </para>
    ///
    /// <para>
    /// <c>Id</c> needs to be unique, so normally is represented by Guid type.
    /// </para>
    ///
    /// <para>
    /// It's also common to add two other columns:
    /// </para>
    /// <list type="bullet">
    /// <item>
    ///     <term><c>Type</c></term>
    ///     <description>information about the stream type. It' mostly used to make debugging easier or some optimizations.</description>
    /// </item>
    /// <item>
    ///     <term><c>Version</c></term>
    ///     <description>auto-incremented value used mostly for optimistic concurrency check</description>
    /// </item>
    /// </list>
    /// <para>
    /// Class provides set of tests verifying if <see cref="EventStore.Init()"/> method initializes <c>Streams</c> table properly.
    /// </para>
    /// </remarks>
    public class Exercise01CreateStreamsTable : IDisposable
    {
        private readonly NpgsqlConnection databaseConnection;
        private readonly PostgresSchemaProvider schemaProvider;

        private const string StreamsTableName = "streams";

        private const string IdColumnName = "id";
        private const string TypeColumnName = "type";
        private const string VersionColumnName = "version";

        /// <summary>
        /// Inits Event Store
        /// </summary>
        public Exercise01CreateStreamsTable()
        {
            databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
            schemaProvider = new PostgresSchemaProvider(databaseConnection);

            // Create Event Store
            var eventStore = new EventStore(databaseConnection);

            // Initialize Event Store
            eventStore.Init();
        }

        /// <summary>
        /// Verifies if Stream table was created
        /// </summary>
        [Fact]
        public void StreamsTable_ShouldBeCreated()
        {
            var streamsTable = schemaProvider.GetTable(StreamsTableName);

            streamsTable.Should().NotBeNull();
            streamsTable.Name.Should().Be(StreamsTableName);
        }

        /// <summary>
        /// Verifies if Stream table has Id column of type Guid
        /// </summary>
        [Fact]
        public void StreamsTable_ShouldHave_IdColumn()
        {
            var idColumn = schemaProvider
                .GetTable(StreamsTableName)
                .GetColumn(IdColumnName);

            idColumn.Should().NotBeNull();
            idColumn.Name.Should().Be(IdColumnName);
            idColumn.Type.Should().Be(Column.GuidType);
        }

        /// <summary>
        /// Verifies if Stream table has Type column of type String
        /// </summary>
        [Fact]
        public void StreamsTable_ShouldHave_TypeColumn_WithStringType()
        {
            var typeColumn = schemaProvider
                .GetTable(StreamsTableName)
                .GetColumn(TypeColumnName);

            typeColumn.Should().NotBeNull();
            typeColumn.Name.Should().Be(TypeColumnName);
            typeColumn.Type.Should().Be(Column.StringType);
        }

        /// <summary>
        /// Verifies if Stream table has Version column of type Long
        /// </summary>
        [Fact]
        public void StreamsTable_ShouldHave_VersionColumn_WithLongType()
        {
            var versionColumn = schemaProvider
                .GetTable(StreamsTableName)
                .GetColumn(VersionColumnName);

            versionColumn.Should().NotBeNull();
            versionColumn.Name.Should().Be(VersionColumnName);
            versionColumn.Type.Should().Be(Column.LongType);
        }

        /// <summary>
        /// Disposes connection to database
        /// </summary>
        public void Dispose()
        {
            databaseConnection.Dispose();
        }
    }
}
