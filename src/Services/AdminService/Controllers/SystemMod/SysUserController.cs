using Perigon.AspNetCore.Models;
using Perigon.AspNetCore.Options;
using Perigon.AspNetCore.Services;
using Microsoft.AspNetCore.RateLimiting;
using Share.Models.Auth;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 系统用户
/// </summary>
public class SysUserController(
        Localizer localizer,
        CacheService cache,
        SysUserManager manager,
        SysDataScopeGroupManager dataScopeGroupManager,
        SysConfigManager sysConfigManager,
        SysRoleManager roleManager,
        IUserContext user,
        ILogger<SysUserController> logger
) : RestControllerBase<SysUserManager>(localizer, manager, user, logger)
{
    private readonly CacheService _cache = cache;
    private readonly SysDataScopeGroupManager _dataScopeGroupManager = dataScopeGroupManager;
    private readonly SysConfigManager _sysConfigManager = sysConfigManager;
    private readonly SysRoleManager _roleManager = roleManager;

    /// <summary>
    /// 登录时，发送邮箱验证码 ✅
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("verifyCode")]
    [AllowAnonymous]
    [EnableRateLimiting(WebConst.Limited)]
    public async Task<ActionResult> SendVerifyCodeAsync(string email)
    {
        if (!await _manager.IsExistAsync(email))
        {
            return BadRequest(Localizer.UserNotFound);
        }

        var captcha = SysUserManager.GetCaptcha();
        var key = WebConst.VerifyCodeCachePrefix + email;
        if (await _cache.GetValueAsync<string>(key) != null)
        {
            return Conflict(Localizer.VerifyCodeAlreadySent);
        }

        // 缓存，默认5分钟过期
        await _cache.SetValueAsync(key, captcha, 60 * 5);
        return Ok();
    }

    /// <summary>
    /// 获取图形验证码 ✅
    /// </summary>
    /// <returns></returns>
    [HttpGet("captcha")]
    [EnableRateLimiting(WebConst.Limited)]
    [AllowAnonymous]
    public ActionResult GetCaptchaImage()
    {
        return File(_manager.GetCaptchaImage(4), "image/png");
    }

    /// <summary>
    /// Get AccessToken ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("authorize")]
    [AllowAnonymous]
    [EnableRateLimiting(WebConst.Limited)]
    public async Task<AccessTokenDto> LoginAsync(SysLoginDto dto)
    {
        // 获取client
        var client =
            HttpContext.Request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;

        var tokenResult = await _manager.LoginAsync(dto, client);
        return tokenResult;
    }

    /// <summary>
    /// Get UserInfo ✅
    /// </summary>
    [HttpGet("userinfo")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfoAsync()
    {
        var user = await _manager.GetSysUserAsync(_user.UserId);
        if (user == null)
        {
            return NotFound(Localizer.NotFoundUser);
        }

        var menus = new List<SysMenu>();
        if (user.SysUserRoles != null)
        {
            menus = await _roleManager.GetSysMenusAsync([.. user.SysUserRoles.Select(ur => ur.Role)]);
        }
        List<SysUserDataScopeGroupItemDto> dataScopeGroups =
            await _dataScopeGroupManager.GetUserDataScopeGroupsAsync(user.Id);

        return new UserInfoDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Roles = user.SysUserRoles?.Select(ur => ur.Role.NameValue).ToArray() ?? [],
            Menus = menus,
            DataScopeGroups = dataScopeGroups,
        };
    }

    /// <summary>
    /// 刷新 token
    /// </summary>
    /// <param name="refreshToken"></param>
    [AllowAnonymous]
    [EnableRateLimiting(WebConst.Limited)]
    [HttpGet("refresh_token")]
    public async Task<ActionResult<AccessTokenDto>> RefreshTokenAsync(string refreshToken)
    {
        var userId = await _cache.GetValueAsync<string>(refreshToken);
        if (userId == null || userId != _user.UserId.ToString())
        {
            return Forbid(Localizer.InvalidToken);
        }

        SysUser? user = await _manager.FindAsync(Guid.Parse(userId));

        if (user == null)
        {
            return Forbid(Localizer.NotFoundUser);
        }
        AccessTokenDto jwtToken = await _manager.GenerateJwtTokenAsync(user);
        // 更新缓存
        var loginPolicy = await _sysConfigManager.GetLoginSecurityPolicyAsync();
        var client = HttpContext.Request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;
        if (loginPolicy.SessionLevel == SessionLevel.OnlyOne)
        {
            client = WebConst.AllPlatform;
        }
        var key = user.GetUniqueKey(WebConst.LoginCachePrefix, client);
        await _cache.SetValueAsync(jwtToken.RefreshToken, user.Id.ToString(), jwtToken.RefreshExpiresIn);
        await _cache.RemoveAsync(refreshToken);
        await _cache.SetValueAsync(key, jwtToken.AccessToken, jwtToken.ExpiresIn);
        return jwtToken;
    }

    /// <summary>
    /// 退出 ✅
    /// </summary>
    /// <returns></returns>
    [HttpPost("logout/{id}")]
    public async Task<ActionResult<bool>> LogoutAsync([FromRoute] Guid id)
    {
        if (await _manager.ExistAsync(id))
        {
            // 清除缓存状态
            await _cache.RemoveAsync(WebConst.LoginCachePrefix + id.ToString());
            return Ok();
        }
        return NotFound();
    }

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<PageList<SysUserItemDto>>> FilterAsync([FromQuery] SysUserFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<SysUser>> AddAsync(SysUserAddDto dto)
    {
        // 角色处理
        List<SysRole>? roles = null;
        if (dto.RoleIds != null && dto.RoleIds.Count != 0)
        {
            roles = await _roleManager.ListAsync(r => dto
                .RoleIds
                .Contains(r.Id));
        }
        var entity = await _manager.AddAsync(dto, roles);
        return Created($"/api/SysUser/{entity.Id}", entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<SysUser>> UpdateAsync(
        [FromRoute] Guid id,
        SysUserUpdateDto dto
    )
    {
        // 角色处理
        List<SysRole>? roles = null;
        if (dto.RoleIds != null)
        {
            roles = await _roleManager.ListAsync(r => dto
                .RoleIds
                .Contains(r.Id));
        }
        var entity = await _manager.UpdateAsync(id, dto, roles);
        return Ok(entity);
    }

    /// <summary>
    /// 修改密码 ✅
    /// </summary>
    /// <returns></returns>
    [HttpPatch("changePassword")]
    public async Task<ActionResult<bool>> ChangePassword(string password, string newPassword)
    {
        if (!await _manager.ExistAsync(_user.UserId))
        {
            return NotFound("");
        }
        SysUser? user = await _manager.FindAsync(_user.UserId);
        return !HashCrypto.Validate(password, user!.PasswordSalt, user.PasswordHash)
            ? Problem(Localizer.InvalidUserOrPassword)
            : await _manager.ChangePasswordAsync(user, newPassword);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SysUserDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.GetAsync(id);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _manager.DeleteAsync([id], false);
        return NoContent();
    }
}
