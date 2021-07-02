﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.EventStoreDB.Serialization;
using Core.Projections;
using EventStore.Client;

namespace Core.EventStoreDB.Events
{
    public static class AggregateStreamExtensions
    {
        public static async Task<T?> AggregateStream<T>(
            this EventStoreClient eventStore,
            Guid id,
            CancellationToken cancellationToken,
            ulong? fromVersion = null
        ) where T : class, IProjection
        {
            var readResult = eventStore.ReadStreamAsync(
                Direction.Forwards,
                StreamNameMapper.ToStreamId<T>(id),
                fromVersion ?? StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            // TODO: consider adding extension method for the aggregation and deserialisation
            var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

            await foreach (var @event in readResult)
            {
                var eventData = @event.Deserialize();

                aggregate.When(eventData!);
            }

            return aggregate;
        }
    }
}
