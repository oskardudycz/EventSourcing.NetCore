using System;

namespace EventStoreBasics;

public interface ISnapshot
{
    Type Handles { get; }
    void Handle(IAggregate aggregate);
}