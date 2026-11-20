using System.Data;
using Entity.SystemMod;
using EntityFramework.AppDbFactory;
using Perigon.PostgreSQL;
using SystemMod.Models.SysOrganizationDtos;

namespace SystemMod.Managers;

/// <summary>
/// 组织结构管理。
/// </summary>
public sealed class SysOrganizationManager(
    AppDbFactory dbContextFactory,
    ILogger<SysOrganizationManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SysOrganization>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// 获取组织树。返回值只包含根节点，子组织通过 Children 返回。
    /// </summary>
    public async Task<List<SysOrganizationItemDto>> ListTreeAsync(CancellationToken cancellationToken = default)
    {
        List<SysOrganization> organizations = await _dbSet
            .OrderBy(organization => organization.Name)
            .ThenBy(organization => organization.Id)
            .ToListAsync(cancellationToken);

        return BuildTree(organizations);
    }

    public Task<SysOrganizationItemDto> AddAsync(
        SysOrganizationAddDto dto,
        CancellationToken cancellationToken = default) => ExecuteSerializableAsync(async () =>
        {
            string name = NormalizeName(dto.Name);
            string? externalId = NormalizeExternalId(dto.ExternalId);
            await EnsureParentExistsAsync(dto.ParentId, cancellationToken);
            await EnsureSiblingNameAvailableAsync(dto.ParentId, name, null, cancellationToken);

            SysOrganization entity = new()
            {
                Name = name,
                ExternalId = externalId,
                ParentId = dto.ParentId
            };
            await InsertAsync(entity, cancellationToken);
            return Map(entity);
        }, cancellationToken);

    public Task<SysOrganizationItemDto> UpdateAsync(
        Guid id,
        SysOrganizationUpdateDto dto,
        CancellationToken cancellationToken = default) => ExecuteSerializableAsync(async () =>
        {
            SysOrganization current = await _dbSet
                .Where(organization => organization.Id == id)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);

            Guid? parentId = dto.ParentIdSpecified ? dto.ParentId : current.ParentId;
            if (parentId == id)
            {
                throw new BusinessException(Localizer.OrganizationParentCycle, StatusCodes.Status409Conflict);
            }

            await EnsureParentExistsAsync(parentId, cancellationToken);
            await EnsureNoCycleAsync(id, parentId, cancellationToken);

            string name = dto.Name is null ? current.Name : NormalizeName(dto.Name);
            string? externalId = dto.ExternalIdSpecified
                ? NormalizeExternalId(dto.ExternalId)
                : current.ExternalId;
            await EnsureSiblingNameAvailableAsync(parentId, name, id, cancellationToken);

            await _dbSet
                .Where(organization => organization.Id == id)
                .ExecuteUpdateAsync(update => update
                    .Set(organization => organization.Name, name)
                    .Set(organization => organization.ExternalId, externalId)
                    .Set(organization => organization.ParentId, parentId), cancellationToken: cancellationToken);

            return new SysOrganizationItemDto
            {
                Id = id,
                Name = name,
                ExternalId = externalId,
                ParentId = parentId
            };
        }, cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => ExecuteSerializableAsync(async () =>
    {
        bool exists = await _dbSet
            .Where(organization => organization.Id == id)
            .AnyAsync(cancellationToken);
        if (!exists)
        {
            throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);
        }

        bool hasChildren = await _dbContext.SysOrganizations
            .Where(organization => organization.ParentId == id)
            .AnyAsync(cancellationToken);
        if (hasChildren)
        {
            throw new BusinessException(Localizer.OrganizationHasChildren, StatusCodes.Status409Conflict);
        }

        bool hasUsers = await _dbContext.SysUserOrganizations
            .Where(link => link.OrganizationId == id)
            .AnyAsync(cancellationToken);
        if (hasUsers)
        {
            throw new BusinessException(Localizer.OrganizationHasUsers, StatusCodes.Status409Conflict);
        }

        await DeleteOrUpdateAsync([id], cancellationToken: cancellationToken);
    }, cancellationToken);

    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet
        .Where(organization => organization.Id == id)
        .AnyAsync();

    private async Task EnsureParentExistsAsync(Guid? parentId, CancellationToken cancellationToken)
    {
        if (parentId is not Guid value)
        {
            return;
        }

        bool exists = await _dbSet
            .Where(organization => organization.Id == value)
            .AnyAsync(cancellationToken);
        if (!exists)
        {
            throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);
        }
    }

    private async Task EnsureSiblingNameAvailableAsync(
        Guid? parentId,
        string name,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        var siblings = _dbSet.Where(organization => organization.ParentId == parentId && organization.Name == name);
        if (excludedId is Guid excluded)
        {
            siblings = siblings.Where(organization => organization.Id != excluded);
        }

        bool duplicate = await siblings.AnyAsync(cancellationToken);
        if (duplicate)
        {
            throw new BusinessException(Localizer.OrganizationNameExists, StatusCodes.Status409Conflict);
        }
    }

    private async Task EnsureNoCycleAsync(Guid id, Guid? parentId, CancellationToken cancellationToken)
    {
        if (parentId is not Guid value)
        {
            return;
        }

        var organizations = await _dbSet.ToListAsync(cancellationToken);
        Dictionary<Guid, Guid?> parents = organizations.ToDictionary(organization => organization.Id, organization => organization.ParentId);
        HashSet<Guid> visited = [];
        Guid? current = value;
        while (current is Guid currentId)
        {
            if (!visited.Add(currentId) || currentId == id)
            {
                throw new BusinessException(Localizer.OrganizationParentCycle, StatusCodes.Status409Conflict);
            }

            current = parents.GetValueOrDefault(currentId);
        }
    }

    private Task<T> ExecuteSerializableAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken) =>
        _dbContext.TransactionAsync(_ => operation(), new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, cancellationToken);

    private Task ExecuteSerializableAsync(Func<Task> operation, CancellationToken cancellationToken) =>
        _dbContext.TransactionAsync(_ => operation(), new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, cancellationToken);

    private static List<SysOrganizationItemDto> BuildTree(IReadOnlyCollection<SysOrganization> organizations)
    {
        Dictionary<Guid, SysOrganizationItemDto> nodes = organizations
            .ToDictionary(organization => organization.Id, Map);
        List<SysOrganizationItemDto> roots = [];

        foreach (SysOrganization organization in organizations)
        {
            SysOrganizationItemDto node = nodes[organization.Id];
            if (organization.ParentId is not Guid parentId || !nodes.TryGetValue(parentId, out SysOrganizationItemDto? parent))
            {
                roots.Add(node);
                continue;
            }

            parent.Children.Add(node);
        }

        return roots;
    }

    private static SysOrganizationItemDto Map(SysOrganization organization) => new()
    {
        Id = organization.Id,
        Name = organization.Name,
        ExternalId = organization.ExternalId,
        ParentId = organization.ParentId
    };

    private static string NormalizeName(string? value)
    {
        string name = value?.Trim() ?? string.Empty;
        if (name.Length == 0)
        {
            throw new BusinessException(Localizer.OrganizationNameRequired, StatusCodes.Status400BadRequest);
        }

        return name;
    }

    private static string? NormalizeExternalId(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized;
    }
}
