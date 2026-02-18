using Core.Ids;
using Marten;
using Marten.Schema.Identity;

namespace Core.Marten.Ids;

public class MartenIdGenerator(IDocumentSession documentSession): IIdGenerator
{
    private readonly IDocumentSession documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));

    public Guid New() => Marten.Schema.Identity.CombGuid.NewGuid();
}
