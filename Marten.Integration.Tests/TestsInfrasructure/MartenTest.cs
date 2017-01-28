using System;
using System.Data;
using Marten.Events;
using Npgsql;

namespace Marten.Integration.Tests.TestsInfrasructure
{
    public abstract class MartenTest : IDisposable
    {
        protected readonly IDocumentSession Session;

        protected IEventStore EventStore => Session.Events;

        private readonly string _schemaName = "sch" + Guid.NewGuid().ToString().Replace("-",string.Empty);

        protected MartenTest()
        {
            Session = DocumentSessionProvider.Get(_schemaName);
        }

        public void Dispose()
        {
            Session?.Dispose();

            var sql = $"DROP SCHEMA {_schemaName} CASCADE;";
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var command = conn.CreateCommand();
                    command.CommandText = sql;

                    command.ExecuteNonQuery();

                    tran.Commit();
                }
            }
        }
    }
}
