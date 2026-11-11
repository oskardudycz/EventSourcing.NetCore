using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.Core;

public abstract class Projection<TDocument>(Database database) where TDocument : class
{
    public abstract Type[] CanHandle();

    protected abstract Guid? GetDocumentId(object @event);

    protected abstract TDocument? Evolve(TDocument? current, object @event);

    public async ValueTask<TDocument?> Handle(object @event, CancellationToken ct = default)
    {
        if (GetDocumentId(@event) is not { } id)
            return null;

        var document = Evolve(await database.Get<TDocument>(id, ct), @event);

        if (document == null)
        {
            await database.Delete<TDocument>(id, ct);
            return null;
        }

        await database.Store(id, document, ct);
        return document;
    }
}
