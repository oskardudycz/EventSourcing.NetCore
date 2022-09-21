using AutoMapper;
using ECommerce.Domain.Core.Entities;
using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Core.Services;

public class Service<TEntity>: IService
    where TEntity : class, IEntity, new()
{
    private readonly DbContext dbContext;
    private readonly IMapper mapper;

    public Service(DbContext dbContext, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

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

    public Task DeleteByIdAsync(Guid id, CancellationToken ct)
    {
        return dbContext.DeleteAndSaveChanges<TEntity>(id, ct);
    }
}
