using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Perigon.PostgreSQL;
using ResourceMod.Models.ResourceDtos;
using ResourceMod.Models.UserFavoriteResourceDtos;
using Share.Exceptions;

namespace ResourceMod.Managers;

/// <summary>当前用户的资源收藏管理。</summary>
public class UserFavoriteResourceManager(
    AppDbFactory dbContextFactory,
    ILogger<UserFavoriteResourceManager> logger,
    IUserContext userContext,
    ResourceManager resourceManager
) : ManagerBase<DefaultDbContext, UserFavoriteResource>(dbContextFactory, userContext, logger)
{
    private readonly ResourceManager _resourceManager = resourceManager;

    public async Task<PageList<UserFavoriteResourceItemDto>> MineAsync(UserFavoriteResourceFilterDto filter)
    {
        IQueryable<Resource> visibleResources = _resourceManager.GetVisibleQuery(_dbContext);
        IQueryable<UserFavoriteResource> query = _dbSet
            .Where(favorite => favorite.UserId == _userContext.UserId)
            .Where(favorite => visibleResources.Any(resource => resource.Id == favorite.ResourceId));
        int count = await query.CountAsync();
        List<UserFavoriteResource> favorites = await query
            .OrderByDescending(favorite => favorite.CreatedTime)
            .Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync();
        Guid[] resourceIds = favorites.Select(item => item.ResourceId).ToArray();
        List<Resource> resources = await visibleResources
            .Where(item => resourceIds.Contains(item.Id)).ToListAsync();
        Dictionary<Guid, ResourceItemDto> items = await BuildItemsAsync(resources);
        return new PageList<UserFavoriteResourceItemDto>
        {
            Count = count,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
            Data = favorites.Where(item => items.ContainsKey(item.ResourceId))
            .Select(item => new UserFavoriteResourceItemDto { Id = item.Id, ResourceId = item.ResourceId, CreatedTime = item.CreatedTime, Resource = items[item.ResourceId] }).ToList()
        };
    }

    public async Task<UserFavoriteResourceDetailDto> GetAsync(Guid resourceId)
    {
        UserFavoriteResource favorite = await _dbSet.FirstOrDefaultAsync(item => item.UserId == _userContext.UserId && item.ResourceId == resourceId)
            ?? throw new BusinessException("收藏记录不存在", StatusCodes.Status404NotFound);
        ResourceDetailDto resource = await _resourceManager.GetAsync(resourceId)
            ?? throw new BusinessException("资源不存在或无权访问", StatusCodes.Status403Forbidden);
        return new UserFavoriteResourceDetailDto { Id = favorite.Id, ResourceId = favorite.ResourceId, CreatedTime = favorite.CreatedTime, Resource = resource };
    }

    public async Task<UserFavoriteResource> AddAsync(UserFavoriteResourceAddDto input)
    {
        if (!await _resourceManager.GetVisibleQuery(_dbContext).AnyAsync(resource => resource.Id == input.ResourceId))
        {
            throw new BusinessException("资源不存在或无权收藏", StatusCodes.Status403Forbidden);
        }

        if (await _dbSet.AnyAsync(favorite => favorite.UserId == _userContext.UserId && favorite.ResourceId == input.ResourceId))
        {
            throw new BusinessException("资源已收藏", StatusCodes.Status409Conflict);
        }

        UserFavoriteResource favorite = new()
        {
            UserId = _userContext.UserId,
            ResourceId = input.ResourceId
        };
        await _dbSet.InsertAsync(favorite);
        return favorite;
    }

    public async Task<bool> RemoveAsync(Guid resourceId)
    {
        UserFavoriteResource favorite = await _dbSet.FirstOrDefaultAsync(item => item.UserId == _userContext.UserId && item.ResourceId == resourceId)
            ?? throw new BusinessException("收藏记录不存在", StatusCodes.Status404NotFound);
        await _dbSet.Where(item => item.Id == favorite.Id).ExecuteUpdateAsync(s => s.Set(item => item.IsDeleted, true));
        return true;
    }

    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet.AnyAsync(favorite => favorite.Id == id && favorite.UserId == _userContext.UserId);

    private async Task<Dictionary<Guid, ResourceItemDto>> BuildItemsAsync(List<Resource> resources)
    {
        if (resources.Count == 0)
        {
            return [];
        }

        Guid[] environmentIds = resources.Select(item => item.EnvironmentId).Distinct().ToArray();
        Guid[] categoryIds = resources.Select(item => item.CategoryId).Distinct().ToArray();
        Guid[] groupIds = resources.Where(item => item.GroupId.HasValue).Select(item => item.GroupId!.Value).Distinct().ToArray();
        Guid[] definitionIds = resources.Select(item => item.DefinitionId).Distinct().ToArray();
        Dictionary<Guid, string> environments = (await _dbContext.ResEnvironments.Where(item => environmentIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id, item => item.Name);
        Dictionary<Guid, string> categories = (await _dbContext.ResCategories.Where(item => categoryIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id, item => item.Name);
        Dictionary<Guid, string> groups = (await _dbContext.ResGroups.Where(item => groupIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id, item => item.Name);
        Dictionary<Guid, string> definitions = (await _dbContext.ResDefinitions.Where(item => definitionIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id, item => item.Name);
        return resources.ToDictionary(resource => resource.Id, resource => new ResourceItemDto
        {
            Id = resource.Id,
            EnvironmentId = resource.EnvironmentId,
            EnvironmentName = environments[resource.EnvironmentId],
            CategoryId = resource.CategoryId,
            CategoryName = categories[resource.CategoryId],
            GroupId = resource.GroupId,
            GroupName = resource.GroupId.HasValue && groups.TryGetValue(resource.GroupId.Value, out string? group) ? group : null,
            DefinitionId = resource.DefinitionId,
            DefinitionName = definitions[resource.DefinitionId],
            TagNames = resource.TagNames.ToList(),
            UpdatedTime = resource.UpdatedTime
        });
    }
}

