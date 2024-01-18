using AutoMapper;
using AutoMapper.QueryableExtensions;
using ECommerce.Domain.Core.Entities;
using ECommerce.Domain.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Core.Services;

public abstract class ReadOnlyOnlyService<TEntity>: IReadOnlyService<TEntity>
    where TEntity : class, IEntity, new()
{
    private readonly IQueryable<TEntity> query;
    private readonly IMapper mapper;

    protected ReadOnlyOnlyService(IQueryable<TEntity> query, IMapper mapper)
    {
        this.query = query;
        this.mapper = mapper;
    }

    public async Task<TResponse> GetByIdAsync<TResponse>(Guid id, CancellationToken ct)
    {
        return mapper.Map<TResponse>(await GetEntityByIdAsync(id, ct));
    }

    public Task<List<TResponse>> GetPagedAsync<TResponse>(CancellationToken ct, int pageNumber = 1, int pageSize = 20)
    {
        return Query()
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectTo<TResponse>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
    public IQueryable<TEntity> Query()
    {
        return query;
    }

    protected async Task<TEntity> GetEntityByIdAsync(Guid id, CancellationToken ct)
    {
        var result = await query.SingleOrDefaultAsync(e => e.Id == id, ct);

        if (result == null)
            throw new ArgumentException($"{typeof(TEntity).Name} with id '{id}' was not found");

        return result;
    }
}
