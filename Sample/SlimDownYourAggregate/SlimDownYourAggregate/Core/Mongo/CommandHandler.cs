using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SlimDownYourAggregate.Core.Mongo;

public class Dummy
{
    [BsonId] public string Id { get; private init; }

    public string Name { get; private set; }

    public static Dummy Create(string name) =>
        new Dummy(ObjectId.GenerateNewId().ToString(), name);

    [BsonConstructor]
    private Dummy(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void SetName(string name)
    {
        Name = name;
    }
}

public record AddDummy(string Name);

public record UpdateDummyName(string Id, string Name);

public class DummyCommandHandler
{
    private readonly MongoRepository<Dummy> repository;

    public DummyCommandHandler(MongoRepository<Dummy> repository) =>
        this.repository = repository;

    public Task Handle(AddDummy command, CancellationToken ct) =>
        repository.AddAsync(Dummy.Create(command.Name), ct);


    public Task Handle(UpdateDummyName command, CancellationToken ct) =>
        repository.GetAndUpdateAsync(
            command.Id,
            state => state.SetName(command.Name),
            ct
        );
}
