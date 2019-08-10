using System;
using Npgsql;

namespace EventStoreBasics
{
    public class EventStore
    {
        private readonly NpgsqlConnection databaseConnection;

        public EventStore(NpgsqlConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public void Init()
        {

        }
    }
}
