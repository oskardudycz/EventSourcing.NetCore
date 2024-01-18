using AutoMapper;
using AutoMapper.QueryableExtensions;
using ECommerce.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Services;

public abstract class ReadOnlyOnlyService<TEntity>: IReadOnlyService<TEntity>
    where TEntity : class, IEntity, new()
{
    private readonly IReadonlyRepository<TEntity> repository;
    private readonly IMapper mapper;

    protected ReadOnlyOnlyService(IReadonlyRepository<TEntity> repository, IMapper mapper)
    {
        this.repository = repository;
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
        return repository.Query();
    }

    protected async Task<TEntity> GetEntityByIdAsync(Guid id, CancellationToken ct)
    {
        var result = await repository.FindByIdAsync(id, ct);

        if (result == null)
            throw new ArgumentException($"{typeof(TEntity).Name} with id '{id}' was not found");

        return result;
    }
}
