using AutoMapper;
using ECommerce.Domain.Core.Entities;
using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Core.Services;

public abstract class Service<TEntity>(DbContext dbContext, IMapper mapper): IService
    where TEntity : class, IEntity, new()
{
    protected readonly DbContext dbContext = dbContext;

    public Task CreateAsync<TCreateRequest>(
        TCreateRequest request,
        CancellationToken ct
    )
    {
        var entity = mapper.Map<TCreateRequest, TEntity>(request);

        return dbContext.AddAndSaveChanges(entity, ct);
    }

    public async Task UpdateAsync<TUpdateRequest>(
        TUpdateRequest request,
        CancellationToken ct
    ) where TUpdateRequest : IUpdateRequest
    {
        await dbContext.UpdateAndSaveChanges<TEntity>(
            request.Id,
            entity => mapper.Map(request, entity),
            ct
        );
    }

    public Task DeleteByIdAsync(Guid id, CancellationToken ct) =>
        dbContext.DeleteAndSaveChanges<TEntity>(id, ct);
}
