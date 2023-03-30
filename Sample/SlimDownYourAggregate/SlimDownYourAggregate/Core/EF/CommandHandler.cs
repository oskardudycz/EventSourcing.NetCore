using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SlimDownYourAggregate.Slimmed;

namespace SlimDownYourAggregate.Core.EF;

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

public class DummyDbContext: DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dummy>();
    }
}

public class DummyCommandHandler
{
    private readonly EFRepository<DummyDbContext, ChemicalReactionModel> repository;

    public DummyCommandHandler(EFRepository<DummyDbContext, ChemicalReactionModel> repository) =>
        this.repository = repository;

    public Task Handle(AddDummy command, CancellationToken ct) =>
        repository.AddAsync(new Slimmed.ChemicalReaction( /**We'd put data from command*/), ct);

    public Task Handle(StartReaction command, CancellationToken ct) =>
        repository.GetAndUpdateAsync(
            command.Id,
            state => ChemicalReactionService.Decide(state.ToChemicalReaction(), command),
            ct
        );
}
