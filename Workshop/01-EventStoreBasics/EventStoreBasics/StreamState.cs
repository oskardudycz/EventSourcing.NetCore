using System;

namespace EventStoreBasics
{
    public class StreamState
    {
        public Guid Id { get; }

        public Type Type { get; }

        public long Version { get; }

        public StreamState(Guid id, Type type, long version)
        {
            Id = id;
            Type = type;
            Version = version;
        }
    }
}
