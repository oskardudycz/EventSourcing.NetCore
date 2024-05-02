using MongoDB.Driver;

namespace SlimDownYourAggregate.Core.Mongo;

public class MongoRepository<T>(IMongoCollection<T> collection)
{
    public async Task<T?> FindAsync(string id, CancellationToken ct) =>
        await collection.Find(Builders<T>.Filter.Eq("_id", id)).SingleOrDefaultAsync(ct);

    public Task AddAsync(T aggregate, CancellationToken ct) =>
        collection.InsertOneAsync(aggregate, new InsertOneOptions(), ct);

    public Task UpdateAsync(string id, T aggregate, CancellationToken ct) =>
        collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), aggregate, cancellationToken: ct);

    public Task DeleteAsync(string id, T aggregate, CancellationToken ct) =>
        collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id), cancellationToken: ct);
}

public static class MongoRepositoryExtensions
{
    public static async Task GetAndUpdateAsync<T>(this MongoRepository<T> repository, string id, Action<T> handle,
        CancellationToken ct)
    {
        var entity = await repository.FindAsync(id, ct);

        if (entity == null)
            throw new InvalidOperationException($"{nameof(T)} with id '{id}' was not found!");

        handle(entity);

        await repository.UpdateAsync(id, entity, ct);
    }
}
