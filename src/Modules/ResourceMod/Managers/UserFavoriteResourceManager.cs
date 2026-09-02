using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using ResourceMod.Models.ResourceDtos;
using ResourceMod.Models.UserFavoriteResourceDtos;
using Share.Exceptions;

namespace ResourceMod.Managers;

/// <summary>
/// 当前用户的资源收藏管理。
/// </summary>
public class UserFavoriteResourceManager(
    AppDbFactory dbContextFactory,
    ILogger<UserFavoriteResourceManager> logger,
    IUserContext userContext,
    ResourceManager resourceManager
) : ManagerBase<DefaultDbContext, UserFavoriteResource>(dbContextFactory, userContext, logger)
{
    private readonly ResourceManager _resourceManager = resourceManager;

    /// <summary>
    /// 查询当前用户的收藏资源。
    /// </summary>
    public async Task<PageList<UserFavoriteResourceItemDto>> MineAsync(
        UserFavoriteResourceFilterDto filter)
    {
        IQueryable<UserFavoriteResource> query = _dbSet
            .Where(favorite => favorite.UserId == _userContext.UserId)
            .Where(favorite => _resourceManager
                .GetVisibleQuery(_dbContext)
                .Any(resource => resource.Id == favorite.ResourceId));

        int count = await query.CountAsync();
        List<UserFavoriteResourceItemDto> data = await query
            .OrderByDescending(favorite => favorite.CreatedTime)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(favorite => new UserFavoriteResourceItemDto
            {
                Id = favorite.Id,
                ResourceId = favorite.ResourceId,
                CreatedTime = favorite.CreatedTime,
                Resource = new ResourceItemDto
                {
                    Id = favorite.Resource.Id,
                    EnvironmentId = favorite.Resource.EnvironmentId,
                    EnvironmentName = favorite.Resource.Environment.Name,
                    CategoryId = favorite.Resource.CategoryId,
                    CategoryName = favorite.Resource.Category.Name,
                    GroupId = favorite.Resource.GroupId,
                    GroupName = favorite.Resource.Group == null ? null : favorite.Resource.Group.Name,
                    DefinitionId = favorite.Resource.DefinitionId,
                    DefinitionName = favorite.Resource.Definition.Name,
                    TagNames = favorite.Resource.TagNames,
                    UpdatedTime = favorite.Resource.UpdatedTime
                }
            })
            .ToListAsync();

        return new PageList<UserFavoriteResourceItemDto>
        {
            Count = count,
            Data = data,
            PageIndex = filter.PageIndex
        };
    }

    /// <summary>
    /// 查询当前用户对指定资源的收藏详情。
    /// </summary>
    public async Task<UserFavoriteResourceDetailDto> GetAsync(Guid resourceId)
    {
        UserFavoriteResource favorite = await _dbSet
            .FirstOrDefaultAsync(item =>
                item.UserId == _userContext.UserId &&
                item.ResourceId == resourceId)
            ?? throw new BusinessException("收藏记录不存在", StatusCodes.Status404NotFound);

        ResourceDetailDto resource = await _resourceManager.GetAsync(resourceId)
            ?? throw new BusinessException("资源不存在或无权访问", StatusCodes.Status403Forbidden);

        return new UserFavoriteResourceDetailDto
        {
            Id = favorite.Id,
            ResourceId = favorite.ResourceId,
            CreatedTime = favorite.CreatedTime,
            Resource = resource
        };
    }

    /// <summary>
    /// 收藏一个当前用户可见的常规资源。
    /// </summary>
    public async Task<UserFavoriteResource> AddAsync(UserFavoriteResourceAddDto input)
    {
        bool visible = await _resourceManager
            .GetVisibleQuery(_dbContext)
            .AnyAsync(resource => resource.Id == input.ResourceId);
        if (!visible)
        {
            throw new BusinessException("资源不存在或无权收藏", StatusCodes.Status403Forbidden);
        }

        bool exists = await _dbSet.AnyAsync(favorite =>
            favorite.UserId == _userContext.UserId &&
            favorite.ResourceId == input.ResourceId);
        if (exists)
        {
            throw new BusinessException("资源已收藏", StatusCodes.Status409Conflict);
        }

        UserFavoriteResource favorite = new()
        {
            UserId = _userContext.UserId,
            ResourceId = input.ResourceId,
            TenantId = _userContext.TenantId
        };
        await _dbSet.AddAsync(favorite);
        await _dbContext.SaveChangesAsync();
        return favorite;
    }

    /// <summary>
    /// 取消当前用户对指定资源的收藏。
    /// </summary>
    public async Task<bool> RemoveAsync(Guid resourceId)
    {
        UserFavoriteResource favorite = await _dbSet
            .FirstOrDefaultAsync(item =>
                item.UserId == _userContext.UserId &&
                item.ResourceId == resourceId)
            ?? throw new BusinessException("收藏记录不存在", StatusCodes.Status404NotFound);

        favorite.IsDeleted = true;
        favorite.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return _dbSet.AnyAsync(favorite =>
            favorite.Id == id && favorite.UserId == _userContext.UserId);
    }
}
