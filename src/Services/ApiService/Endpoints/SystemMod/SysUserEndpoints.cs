using Entity.SystemMod;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.RateLimiting;
using Perigon.AspNetCore.Models;
using Perigon.AspNetCore.Options;
using Perigon.AspNetCore.Services;
using Perigon.AspNetCore.Utils;
using Share;
using Share.Models.Auth;
using SystemMod.Models;
using SystemMod.Managers;
using SystemMod.Models.SysUserDtos;

namespace ApiService.Endpoints.SystemMod;

public sealed class SysUserEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysUser", "SysUser", WebConst.User);
        group.MapPost("/verifyCode", SendVerifyCodeAsync).AllowAnonymous().RequireRateLimiting(WebConst.Limited);
        group.MapGet("/captcha", GetCaptchaImage).AllowAnonymous().RequireRateLimiting(WebConst.Limited);
        group.MapPost("/authorize", LoginAsync).AllowAnonymous().RequireRateLimiting(WebConst.Limited);
        group.MapGet("/userinfo", GetUserInfoAsync);
        group.MapGet("/refresh_token", RefreshTokenAsync).AllowAnonymous().RequireRateLimiting(WebConst.Limited);
        group.MapPost("/logout/{id}", LogoutAsync);
        group.MapGet("", FilterAsync).RequireAuthorization(WebConst.SuperAdmin);
        group.MapPost("", AddAsync).RequireAuthorization(WebConst.SuperAdmin);
        group.MapPatch("/changePassword", ChangePasswordAsync);
        group.MapPatch("/{id}", UpdateAsync).RequireAuthorization(WebConst.SuperAdmin);
        group.MapGet("/{id}", GetDetailAsync);
        group.MapDelete("/{id}", DeleteAsync).RequireAuthorization(WebConst.SuperAdmin);
    }

    private static async Task<IResult> SendVerifyCodeAsync(
        string email,
        SysUserManager manager,
        CacheService cache,
        CancellationToken cancellationToken)
    {
        if (!await manager.IsExistAsync(email))
        {
            return TypedResults.BadRequest(Localizer.UserNotFound);
        }

        var key = WebConst.VerifyCodeCachePrefix + email;
        if (await cache.GetValueAsync<string>(key) is not null)
        {
            return TypedResults.Conflict(Localizer.VerifyCodeAlreadySent);
        }

        await cache.SetValueAsync(key, SysUserManager.GetCaptcha(), 60 * 5);
        return TypedResults.Ok();
    }

    private static IResult GetCaptchaImage(SysUserManager manager) =>
        TypedResults.File(manager.GetCaptchaImage(4), "image/png");

    private static async Task<AccessTokenDto> LoginAsync(
        SysLoginDto dto,
        SysUserManager manager,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var client = request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;
        return await manager.LoginAsync(dto, client);
    }

    private static async Task<IResult> GetUserInfoAsync(
        SysUserManager manager,
        SysRoleManager roleManager,
        SysDataScopeGroupManager dataScopeGroupManager,
        IUserContext user,
        CancellationToken cancellationToken)
    {
        var entity = await manager.GetSysUserAsync(user.UserId);
        if (entity is null)
        {
            return TypedResults.NotFound(Localizer.NotFoundUser);
        }

        var menus = entity.SysUserRoles is null
            ? []
            : await roleManager.GetSysMenusAsync([.. entity.SysUserRoles.Select(ur => ur.Role)]);
        var dataScopeGroups = await dataScopeGroupManager.GetUserDataScopeGroupsAsync(entity.Id);

        return TypedResults.Ok(new UserInfoDto
        {
            Id = entity.Id,
            Username = entity.UserName ?? string.Empty,
            Roles = entity.SysUserRoles?.Select(ur => ur.Role.NameValue).ToArray() ?? [],
            Menus = menus,
            DataScopeGroups = dataScopeGroups,
        });
    }

    private static async Task<IResult> RefreshTokenAsync(
        string refreshToken,
        SysUserManager manager,
        SysConfigManager configManager,
        CacheService cache,
        IUserContext user,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await cache.GetValueAsync<string>(refreshToken);
        if (userId is null || userId != user.UserId.ToString())
        {
            return TypedResults.Forbid();
        }

        var entity = await manager.FindAsync(Guid.Parse(userId));
        if (entity is null)
        {
            return TypedResults.Forbid();
        }

        var token = await manager.GenerateJwtTokenAsync(entity);
        var policy = await configManager.GetLoginSecurityPolicyAsync();
        var client = request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;
        if (policy.SessionLevel == SessionLevel.OnlyOne)
        {
            client = WebConst.AllPlatform;
        }

        var key = entity.GetUniqueKey(WebConst.LoginCachePrefix, client);
        await cache.SetValueAsync(token.RefreshToken, entity.Id.ToString(), token.RefreshExpiresIn);
        await cache.RemoveAsync(refreshToken);
        await cache.SetValueAsync(key, token.AccessToken, token.ExpiresIn);
        return TypedResults.Ok(token);
    }

    private static async Task<IResult> LogoutAsync(
        Guid id,
        SysUserManager manager,
        CacheService cache,
        CancellationToken cancellationToken)
    {
        if (!await manager.ExistAsync(id))
        {
            return TypedResults.NotFound();
        }

        await cache.RemoveAsync(WebConst.LoginCachePrefix + id.ToString());
        return TypedResults.Ok();
    }

    private static Task<PageList<SysUserItemDto>> FilterAsync(
        [AsParameters] SysUserFilterDto filter,
        SysUserManager manager,
        CancellationToken cancellationToken) => manager.ToPageAsync(filter);

    private static async Task<IResult> AddAsync(
        SysUserAddDto dto,
        SysUserManager manager,
        SysRoleManager roleManager,
        CancellationToken cancellationToken)
    {
        List<SysRole>? roles = dto.RoleIds is { Count: > 0 }
            ? await roleManager.ListAsync(r => dto.RoleIds!.Contains(r.Id))
            : null;
        var entity = await manager.AddAsync(dto, roles);
        return TypedResults.Created($"/api/SysUser/{entity.Id}", entity);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SysUserUpdateDto dto,
        SysUserManager manager,
        SysRoleManager roleManager,
        CancellationToken cancellationToken)
    {
        List<SysRole>? roles = dto.RoleIds is not null
            ? await roleManager.ListAsync(r => dto.RoleIds.Contains(r.Id))
            : null;
        return TypedResults.Ok(await manager.UpdateAsync(id, dto, roles));
    }

    private static async Task<IResult> ChangePasswordAsync(
        string password,
        string newPassword,
        SysUserManager manager,
        IUserContext user,
        Localizer localizer,
        CancellationToken cancellationToken)
    {
        if (!await manager.ExistAsync(user.UserId))
        {
            return TypedResults.NotFound(string.Empty);
        }

        var entity = await manager.FindAsync(user.UserId);
        if (entity is null || !HashCrypto.Validate(password, entity.PasswordSalt, entity.PasswordHash))
        {
            return TypedResults.Problem(localizer.Get(Localizer.InvalidUserOrPassword));
        }

        return TypedResults.Ok(await manager.ChangePasswordAsync(entity, newPassword));
    }

    private static async Task<IResult> GetDetailAsync(
        Guid id,
        SysUserManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        SysUserManager manager,
        CancellationToken cancellationToken)
    {
        await manager.DeleteAsync([id], false);
        return TypedResults.NoContent();
    }
}
