using System;
using Ardalis.GuardClauses;
using Marten;

namespace Core.Ids
{
    public class MartenIdGenerator : IIdGenerator
    {
        private readonly IDocumentSession documentSession;

        public MartenIdGenerator(IDocumentSession documentSession)
        {
            Guard.Against.Null(documentSession, nameof(documentSession));

            this.documentSession = documentSession;
        }

        public Guid New() => documentSession.Events.StartStream().Id;
    }
}
