using Npgsql;

namespace EventStoreBasics;

public class EventStore(NpgsqlConnection databaseConnection): IDisposable, IEventStore
{
    public void Init()
    {
        // See more in Greg Young's "Building an Event Storage" article https://cqrs.wordpress.com/documents/building-event-storage/
        CreateStreamsTable();
    }

    private void CreateStreamsTable()
    {
        throw new NotImplementedException("Add here create table sql run with Dapper");
    }

    public void Dispose()
    {
        databaseConnection.Dispose();
    }
}
