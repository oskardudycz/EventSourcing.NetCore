using System;
using Ardalis.GuardClauses;
using Core.Aggregates;
using Marten;

namespace Core.Ids
{
    public class MartenAggregateIdGenerator<T> : IAggregateIdGenerator<T> where T : class, IAggregate, new()
    {
        private readonly IDocumentSession documentSession;

        public MartenAggregateIdGenerator(IDocumentSession documentSession)
        {
            Guard.Against.Null(documentSession, nameof(documentSession));

            this.documentSession = documentSession;
        }

        public Guid New() => documentSession.Events.StartStream<T>().Id;
    }
}
