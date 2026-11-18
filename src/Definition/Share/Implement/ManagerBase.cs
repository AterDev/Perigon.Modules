using Entity;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Perigon.PostgreSQL;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Share.Implement;

public abstract class ManagerBase(ILogger logger)
{
    protected readonly ILogger _logger = logger;
}

public abstract class ManagerBase<TDbContext>(TDbContext dbContext, ILogger logger) : ManagerBase(logger)
    where TDbContext : ContextBase
{
    protected readonly TDbContext _dbContext = dbContext;
}

/// <summary>Tenant-scoped operations without change tracking.</summary>
public abstract class ManagerBase<TDbContext, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity> : IDisposable
    where TDbContext : ContextBase
    where TEntity : class, IEntityBase, new()
{
    protected IQueryable<TEntity> Queryable { get; set; }
    private bool _ignoreQueryFilter;
    protected bool IgnoreQueryFilter
    {
        get => _ignoreQueryFilter;
        set
        {
            _ignoreQueryFilter = value;
            Queryable = value ? _dbContext.SetWithoutFilters<TEntity>(ContextBase.SoftDeletionFilterName) : _dbSet;
        }
    }
    protected readonly ILogger _logger;
    protected TDbContext _dbContext;
    protected DbSet<TEntity> _dbSet;
    protected readonly IUserContext _userContext;
    private readonly AppDbFactory _dbContextFactory;
    protected bool IsTenantScoped => typeof(TEntity) != typeof(Tenant) && typeof(ITenantEntityBase).IsAssignableFrom(typeof(TEntity));

    protected ManagerBase(AppDbFactory dbContextFactory, IUserContext userContext, ILogger logger, bool allowCatalogContext = false)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _userContext = userContext;
        if (IsTenantScoped && userContext.TenantId == Guid.Empty && !allowCatalogContext)
            throw new InvalidOperationException($"A TenantId is required to create a manager for {typeof(TEntity).Name}.");
        Guid? tenantId = IsTenantScoped && userContext.TenantId != Guid.Empty ? userContext.TenantId : null;
        _dbContext = dbContextFactory.CreateDbContext(tenantId) as TDbContext
            ?? throw new InvalidOperationException($"Unsupported context type {typeof(TDbContext).Name}.");
        _dbSet = _dbContext.Set<TEntity>();
        Queryable = _dbSet;
    }

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbSet.Where(entity => entity.Id == id).FirstOrDefaultAsync(cancellationToken);

    protected async Task<TDto?> FindAsync<TDto>(Func<TEntity, TDto> mapper, Expression<Func<TEntity, bool>>? whereExp = null, CancellationToken cancellationToken = default) where TDto : class
    {
        var query = whereExp is null ? (IQueryable<TEntity>)_dbSet : _dbSet.Where(whereExp);
        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        return entity is null ? null : mapper(entity);
    }

    public Task<bool> ExistAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbSet.Where(entity => entity.Id == id).AnyAsync(cancellationToken);

    protected async Task<List<TDto>> ListAsync<TDto>(Func<TEntity, TDto> mapper, Expression<Func<TEntity, bool>>? whereExp = null, CancellationToken cancellationToken = default) where TDto : class
    {
        IQueryable<TEntity> query = IgnoreQueryFilter ? _dbContext.SetWithoutFilters<TEntity>(ContextBase.SoftDeletionFilterName) : _dbSet;
        if (whereExp is not null) query = query.Where(whereExp);
        return (await query.ToListAsync(cancellationToken)).Select(mapper).ToList();
    }

    public async Task<PageList<TItem>> PageListAsync<TFilter, TItem>(TFilter filter, Func<TEntity, TItem> mapper, CancellationToken cancellationToken = default)
        where TFilter : FilterBase where TItem : class
    {
        try
        {
            var ordered = Queryable.OrderByDescending(item => item.CreatedTime);
            var count = await ordered.CountAsync(cancellationToken);
            var entities = await ordered.Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync(cancellationToken);
            return new PageList<TItem> { Count = count, Data = entities.Select(mapper).ToList(), PageIndex = filter.PageIndex.GetValueOrDefault(1) };
        }
        finally { ResetQuery(); }
    }

    protected async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (IsTenantScoped) ((ITenantEntityBase)entity).TenantId = RequireTenantId();
        await _dbSet.InsertAsync(entity, returning: false, cancellationToken: cancellationToken);
    }

    protected async Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var rows = entities.ToArray();
        foreach (var entity in rows) if (IsTenantScoped) ((ITenantEntityBase)entity).TenantId = RequireTenantId();
        await _dbSet.BulkInsertAsync(rows, cancellationToken: cancellationToken);
    }

    protected Task<int> DeleteOrUpdateAsync(IEnumerable<Guid> ids, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        var keys = ids.Distinct().ToArray();
        if (keys.Length == 0) return Task.FromResult(0);
        var query = _dbSet.Where(item => keys.Contains(item.Id));
        return softDelete
            ? query.ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true), cancellationToken: cancellationToken)
            : query.ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default) =>
        await _dbContext.TransactionAsync(_ => operation(), cancellationToken);
    protected async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default) =>
        await _dbContext.TransactionAsync(_ => operation(), cancellationToken);
    protected void ResetQuery() => Queryable = IgnoreQueryFilter ? _dbContext.SetWithoutFilters<TEntity>(ContextBase.SoftDeletionFilterName) : _dbSet;
    protected IQueryable<TEntity> ApplyTenantFilter(IQueryable<TEntity> query) => query;
    protected Guid RequireTenantId() => _userContext.TenantId != Guid.Empty ? _userContext.TenantId : throw new InvalidOperationException("A TenantId is required for tenant-scoped data.");
    protected void BindTenantContext(Guid tenantId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("A tenant id is required.");
        if (_dbContext.CurrentTenantId == tenantId) return;
        if (_dbContext.CurrentTenantId is not null) throw new InvalidOperationException("A context already bound to a tenant cannot be rebound.");
        var context = _dbContextFactory.CreateDbContext(tenantId) as TDbContext
            ?? throw new InvalidOperationException($"Unsupported context type {typeof(TDbContext).Name}.");
        _dbContext.Dispose();
        _dbContext = context;
        _dbSet = _dbContext.Set<TEntity>();
        ResetQuery();
    }
    public abstract Task<bool> HasPermissionAsync(Guid id);
    public void Dispose() => _dbContext.Dispose();
}

