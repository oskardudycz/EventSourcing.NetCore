using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SlimDownYourAggregate.Slimmed;

namespace SlimDownYourAggregate.Core.EF;

public class ChemicalReactionDbContext: DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChemicalReactionModel>();
    }
}

public class ChemicalReactionCommandHandler(EFRepository<ChemicalReactionDbContext, ChemicalReactionModel> repository)
{
    public Task Handle(ChemicalReactionCommand command, CancellationToken ct) =>
        repository.GetAndUpdateAsync(
            command.Id,
            state => ChemicalReactionService.Decide(command, state.ToChemicalReaction()),
            ct
        );
}
