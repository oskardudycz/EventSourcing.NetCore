using AutoMapper;
using AutoMapper.QueryableExtensions;
using ECommerce.Core.Entities;
using ECommerce.Core.Repositories;
using ECommerce.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Services;

public abstract class CRUDService<TEntity>: ICRUDService
    where TEntity : class, IEntity, new()
{
    private readonly ICRUDRepository<TEntity> repository;
    private readonly IMapper mapper;

    protected CRUDService(ICRUDRepository<TEntity> repository, IMapper mapper)
    {
        this.repository = repository;
        this.mapper = mapper;
    }

    public async Task<TCreateResponse> CreateAsync<TCreateRequest, TCreateResponse>(
        TCreateRequest request,
        CancellationToken ct
    )
    {
        var entity = mapper.Map<TCreateRequest, TEntity>(request);

        repository.Add(entity);

        await repository.SaveChangesAsync(ct);

        return await GetByIdAsync<TCreateResponse>(entity.Id, ct);
    }

    public async Task<TUpdateResponse> UpdateAsync<TUpdateRequest, TUpdateResponse>(
        TUpdateRequest request,
        CancellationToken ct
    ) where TUpdateRequest : IUpdateRequest
    {
        var entity = await GetEntityByIdAsync(request.Id, ct);

        entity = mapper.Map(request, entity);

        repository.Update(entity);

        await repository.SaveChangesAsync(ct);

        return await GetByIdAsync<TUpdateResponse>(entity.Id, ct);
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await GetEntityByIdAsync(id, ct);

        repository.Delete(entity);

        await repository.SaveChangesAsync(ct);

        return true;
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
