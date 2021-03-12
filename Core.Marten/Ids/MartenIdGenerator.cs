using System;
using Marten;
using Marten.Schema.Identity;

namespace Core.Ids
{
    public class MartenIdGenerator : IIdGenerator
    {
        private readonly IDocumentSession documentSession;

        public MartenIdGenerator(IDocumentSession documentSession)
        {
            this.documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));
        }

        public Guid New() => CombGuidIdGeneration.NewGuid();
    }
}
