using EntityFramework.AppDbFactory;
using Entity;
using Share.Models.Auth;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Perigon.AspNetCore.Options;
using System.Text.Json;
using SystemMod.Serialization;
using SystemMod.Models.SysUserDtos;

namespace SystemMod.Managers;

public class SysUserManager(
    AppDbFactory dbContextFactory,
    CacheService cache,
    JwtService jwtService,
    SystemLogService logService,
    ILogger<SysUserManager> logger,
    IUserContext userContext,
    Localizer localizer,
    IConfiguration configuration,
    IServiceProvider serviceProvider
) : ManagerBase<DefaultDbContext, SysUser>(
    dbContextFactory,
    userContext,
    logger
)
{
    private static SysUserItemDto MapItem(SysUser x) => new() { Id = x.Id, UserName = x.UserName!, RealName = x.RealName, Email = x.Email, LastLoginTime = x.LastLoginTime, Sex = (GenderType)x.Sex, CreatedTime = x.CreatedTime };
    private static SysUserDetailDto MapDetail(SysUser x) => new() { Id = x.Id, UserName = x.UserName!, RealName = x.RealName, Email = x.Email, EmailConfirmed = x.EmailConfirmed, PhoneNumber = x.PhoneNumber, PhoneNumberConfirmed = x.PhoneNumberConfirmed, TwoFactorEnabled = x.TwoFactorEnabled, LockoutEnd = x.LockoutEnd, LockoutEnabled = x.LockoutEnabled, AccessFailedCount = x.AccessFailedCount, LastLoginTime = x.LastLoginTime, RetryCount = x.RetryCount, Avatar = x.Avatar, Sex = (GenderType)x.Sex, CreatedTime = x.CreatedTime, UpdatedTime = x.UpdatedTime };
    private readonly CacheService _cache = cache;
    private readonly SystemLogService _logService = logService;
    private readonly Localizer _localizer = localizer;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private SysUserRoleManager UserRoleManager =>
        _serviceProvider.GetRequiredService<SysUserRoleManager>();

    /// <summary>
    /// 获取验证码
    /// 也可自己实现图片验证码
    /// </summary>
    /// <param name="length">验证码长度</param>
    /// <returns></returns>
    public static string GetCaptcha(int length = 6)
    {
        return HashCrypto.GetRandom(length);
    }

    /// <summary>
    /// 获取图形验证码
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public byte[] GetCaptchaImage(int length = 4)
    {
        var code = GetCaptcha(length);
        var width = length * 20;
        return ImageHelper.GenerateImageCaptcha(code, width);
    }

    /// <summary>
    /// 登录安全政策验证
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="user"></param>
    /// <param name="loginPolicy"></param>
    /// <returns></returns>
    public async Task ValidateLoginAsync(
        SysLoginDto dto,
        SysUser user,
        LoginSecurityPolicyOption loginPolicy
    )
    {
        user.LastLoginTime = DateTimeOffset.UtcNow;

        if (loginPolicy.IsEnable)
        {
            // 刷新锁定状态
            var lastLoginTime = user.LastLoginTime?.ToLocalTime() ?? DateTimeOffset.Now;
            if ((DateTimeOffset.Now - lastLoginTime).Days >= 1)
            {
                user.RetryCount = 0;
                if (user.LockoutEnabled)
                {
                    user.LockoutEnabled = false;
                }
            }

            // 锁定状态
            if (user.LockoutEnabled || user.RetryCount >= loginPolicy.LoginRetry)
            {
                user.LockoutEnabled = true;
                throw new BusinessException(Localizer.LockAccountForManyTimes);
            }

            // 验证码处理
            if (loginPolicy.IsNeedVerifyCode)
            {
                if (dto.VerifyCode == null)
                {
                    user.RetryCount++;
                    throw new BusinessException(Localizer.InvalidVerifyCode);
                }
                var key = WebConst.VerifyCodeCachePrefix + user.Email;
                var code = await _cache.GetValueAsync<string>(key);
                if (code == null)
                {
                    user.RetryCount++;
                    throw new BusinessException(Localizer.VerifyCodeExpired);
                }
                if (!code.Equals(dto.VerifyCode))
                {
                    await _cache.RemoveAsync(key);
                    user.RetryCount++;
                    throw new BusinessException(Localizer.InvalidVerifyCode);
                }
            }

            // 密码过期时间
            if ((DateTimeOffset.UtcNow - user.LastPwdEditTime).TotalDays > loginPolicy.PasswordExpired)
            {
                user.RetryCount++;
                throw new BusinessException(Localizer.PasswordExpired);
            }
        }

        if (!HashCrypto.Validate(dto.Password, user.PasswordSalt, user.PasswordHash))
        {
            user.RetryCount++;
            throw new BusinessException(
                Localizer.PasswordInvalid,
                StatusCodes.Status401Unauthorized
            );
        }
    }

    /// <summary>
    /// 生成jwtToken
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<AccessTokenDto> GenerateJwtTokenAsync(SysUser user)
    {
        // 通过显式关联表查询用户角色。
        List<SysRole> assignedRoles = await _dbContext.SysRoles
            .Where(role =>
                _dbContext.SysUserRoles.Any(userRole =>
                    userRole.UserId == user.Id &&
                    userRole.RoleId == role.Id))
            .ToListAsync();
        List<string> roles = assignedRoles
            .Select(role => role.NameValue)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        List<Guid> roleIds = assignedRoles.Select(role => role.Id).ToList();
        if (!roles.Contains(WebConst.User))
        {
            roles.Add(WebConst.User);
        }

        List<Claim> claims = [
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName??string.Empty),
        ];
        claims.AddRange(roleIds.Select(roleId => new Claim(
            CustomClaimTypes.RoleId,
            roleId.ToString())));
        jwtService.Claims = claims;
        var token = jwtService.GetToken(user.Id.ToString(), [.. roles]);

        return new AccessTokenDto
        {
            AccessToken = token,
            ExpiresIn = jwtService.ExpiredSecond,
            RefreshToken = JwtService.GetRefreshToken(),
            RefreshExpiresIn = jwtService.RefreshExpiredSecond,
        };
    }

    /// <summary>
    /// 更新密码
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public async Task<bool> ChangePasswordAsync(SysUser user, string newPassword)
    {
        user.PasswordSalt = HashCrypto.BuildSalt();
        user.PasswordHash = HashCrypto.GeneratePwd(newPassword, user.PasswordSalt);
        user.LastPwdEditTime = DateTimeOffset.UtcNow;
        return await _dbSet
            .Where(item => item.Id == user.Id)
            .ExecuteUpdateAsync(update => update
                .Set(item => item.PasswordSalt, user.PasswordSalt)
                .Set(item => item.PasswordHash, user.PasswordHash)
                .Set(item => item.LastPwdEditTime, user.LastPwdEditTime)) > 0;
    }

    public async Task<SysUserOptionDto[]> SearchOptionsAsync(string? search, CancellationToken cancellationToken)
    {
        search = search?.Trim();
        if (search is not { Length: >= 2 and <= 100 })
        {
            return [];
        }

        bool isId = Guid.TryParse(search, out Guid id);
        IQueryable<SysUser> query = isId
            ? _dbSet.Where(user => user.Id == id)
            : _dbSet.Where(user =>
                (user.UserName != null && user.UserName.Contains(search))
                || (user.RealName != null && user.RealName.Contains(search))
                || user.Email.Contains(search));
        var users = await query.OrderBy(user => user.UserName).ThenBy(user => user.Id)
            .Take(20).ToListAsync(cancellationToken);
        return users.Select(user => new SysUserOptionDto(
            user.Id,
            string.IsNullOrWhiteSpace(user.RealName)
                ? user.UserName ?? user.Id.ToString()
                : $"{user.RealName} ({user.UserName})")).ToArray();
    }

    public async Task<PageList<SysUserItemDto>> ToPageAsync(SysUserFilterDto filter)
    {
        Queryable = Queryable.WhereNotNull(
            filter.UserName,
            q =>
                (q.UserName != null && q.UserName.Contains(filter.UserName!))
                || (q.PhoneNumber != null && q.PhoneNumber.Contains(filter.UserName!))
                || q.Email.Contains(filter.UserName!)
        );

        if (filter.RoleId != null)
        {
            var roleLinks = await _dbContext.SysUserRoles
                .Where(link => link.RoleId == filter.RoleId)
                .ToListAsync();
            var roleUserIds = roleLinks.Select(link => link.UserId).Distinct().ToArray();
            Queryable = Queryable.Where(q => roleUserIds.Contains(q.Id));
        }
        return await PageListAsync<SysUserFilterDto, SysUserItemDto>(filter, MapItem);
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<bool> IsExistAsync(string email)
    {
        return await Queryable.Where(q => q.Email == email).AnyAsync();
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SysUser?> GetOwnedAsync(Guid id)
    {
        IQueryable<SysUser> query = _dbSet.Where(q => q.Id == id);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public async Task DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        await base.DeleteOrUpdateAsync(ids, softDelete);
    }

    /// <summary>
    /// 验证密码复杂度
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> ValidatePasswordAsync(string password)
    {
        var loginPolicy = await GetLoginSecurityPolicyAsync();
        // 密码复杂度校验
        var pwdReg = loginPolicy.PasswordLevel switch
        {
            PasswordLevel.Simple => RegexConst.SimplePasswordRegex,
            PasswordLevel.Normal => RegexConst.NormalPasswordRegex,
            PasswordLevel.Strict => RegexConst.StrongPasswordRegex,
            _ => RegexConst.StrongPasswordRegex,
        };
        return Regex.IsMatch(password, pwdReg);
    }

    public async Task<SysUser?> GetSysUserAsync(Guid id)
    {
        var user = await Queryable.Where(q => q.Id == id).FirstOrDefaultAsync();
        return user is null ? null : await LoadUserRolesAsync(user);
    }

    // expose DTO getter for controllers
    public async Task<SysUserDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync(MapDetail, d => d.Id == id);
    }

    public async Task<AccessTokenDto> LoginAsync(SysLoginDto dto, string client)
    {

        // 查询用户
        var user = await _dbSet
            .Where(u => u.Email == dto.Email)
            .FirstOrDefaultAsync() ?? throw new BusinessException(Localizer.UserNotExists);
        await LoadUserRolesAsync(user);
        try
        {
            var loginPolicy = await GetLoginSecurityPolicyAsync();
            await ValidateLoginAsync(dto, user, loginPolicy);

            // 验证成功，生成token
            AccessTokenDto jwtToken = await GenerateJwtTokenAsync(user);
            if (loginPolicy.SessionLevel == SessionLevel.OnlyOne)
            {
                client = WebConst.AllPlatform;
            }
            var key = user.GetUniqueKey(WebConst.LoginCachePrefix, client);

            var expiredSeconds =
                loginPolicy.SessionExpiredSeconds == 0
                    ? jwtToken.ExpiresIn
                    : loginPolicy.SessionExpiredSeconds;

            // 缓存
            await _cache.SetValueAsync(key, jwtToken.AccessToken, expiredSeconds);
            await _cache.SetValueAsync(
                jwtToken.RefreshToken,
                user.Id.ToString(),
                jwtToken.RefreshExpiresIn
            );

            await _logService.NewLog(
                Localizer.LoginAction,
                UserActionType.Login,
                Localizer.LoginSuccess,
                user.UserName,
                user.Id);
            return jwtToken;
        }
        catch (BusinessException)
        {
            // 密码错误等业务异常，直接抛出
            throw;
        }
        catch (Exception ex)
        {
            // 其他异常
            _logger.LogError(ex, "An unexpected error occurred during login.");
            throw;
        }
        finally
        {
            // 确保用户信息（如重试次数、最后登录时间）被保存
            await _dbSet
                .Where(item => item.Id == user.Id)
                .ExecuteUpdateAsync(update => update
                    .Set(item => item.LastLoginTime, user.LastLoginTime)
                    .Set(item => item.RetryCount, user.RetryCount)
                    .Set(item => item.LockoutEnabled, user.LockoutEnabled));
        }
    }

    public async Task<SysUser> AddAsync(SysUserAddDto dto, List<SysRole>? roles)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            SysUser entity = new()
            {
                UserName = dto.UserName,
                RealName = dto.RealName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Avatar = dto.Avatar,
                Sex = (Sex)dto.Sex
            };
            // 密码处理
            entity.PasswordSalt = HashCrypto.BuildSalt();
            entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);

            await InsertAsync(entity);

            // 使用中间表管理器处理角色关联，提高性能
            if (roles != null && roles.Count > 0)
            {
                var roleIds = roles.Select(r => r.Id).ToList();
                await UserRoleManager.SetUserRolesAsync(entity.Id, roleIds, _dbContext);
            }

            return entity;
        });
    }

    public async Task<SysUser> UpdateAsync(
        Guid id,
        SysUserUpdateDto dto,
        List<SysRole>? roles
    )
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var current =
                await FindAsync(id) ?? throw new BusinessException(Localizer.UserNotFound);

            // 权限验证，利用 IUserContext
            if (!CanUserModify(current))
            {
                throw new BusinessException(
                    Localizer.InsufficientPermissions,
                    StatusCodes.Status403Forbidden
                );
            }

            // Perigon PostgreSQL does not track entities; materialize the current
            // row and issue one explicit update for the supplied fields.
            if (dto.UserName is not null)
            {
                current.UserName = dto.UserName;
            }

            if (dto.RealName is not null)
            {
                current.RealName = dto.RealName;
            }

            if (dto.Email is not null)
            {
                current.Email = dto.Email;
            }

            if (dto.PhoneNumber is not null)
            {
                current.PhoneNumber = dto.PhoneNumber;
            }

            if (dto.Avatar is not null)
            {
                current.Avatar = dto.Avatar;
            }

            if (dto.Sex is GenderType sex)
            {
                current.Sex = (Sex)sex;
            }

            if (dto.UserName is not null || dto.RealName is not null || dto.Email is not null
                || dto.PhoneNumber is not null || dto.Avatar is not null || dto.Sex is not null)
            {
                await _dbSet
                    .Where(u => u.Id == id)
                    .ExecuteUpdateAsync(update => update
                        .Set(u => u.UserName, current.UserName)
                        .Set(u => u.RealName, current.RealName)
                        .Set(u => u.Email, current.Email)
                        .Set(u => u.PhoneNumber, current.PhoneNumber)
                        .Set(u => u.Avatar, current.Avatar)
                        .Set(u => u.Sex, current.Sex));
            }

            // 如需修改密码，单独更新密码相关字段
            if (dto.Password != null)
            {
                if (!await ValidatePasswordAsync(dto.Password))
                {
                    throw new BusinessException(
                        Localizer.PasswordComplexityNotMet,
                        StatusCodes.Status400BadRequest
                    );
                }

                var newSalt = HashCrypto.BuildSalt();
                var newHash = HashCrypto.GeneratePwd(dto.Password, newSalt);
                var now = DateTimeOffset.UtcNow;

                await _dbSet
                    .Where(u => u.Id == id)
                    .ExecuteUpdateAsync(u => u
                        .Set(e => e.PasswordSalt, newSalt)
                        .Set(e => e.PasswordHash, newHash)
                        .Set(e => e.LastPwdEditTime, now));
            }

            // 使用中间表管理器处理角色关联，提高性能
            if (roles != null)
            {
                var roleIds = roles.Select(r => r.Id).ToList();
                await UserRoleManager.SetUserRolesAsync(current.Id, roleIds, _dbContext);
            }

            return current;
        });
    }

    /// <summary>
    /// 验证用户是否可以修改
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private bool CanUserModify(SysUser user)
    {
        // 超级管理员可以修改所有用户，普通用户只能修改自己
        return _userContext.IsRole(WebConst.SuperAdmin) || _userContext.UserId == user.Id;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id);
        return await query.AnyAsync();
    }

    private async Task<SysUser> LoadUserRolesAsync(SysUser user)
    {
        var links = await _dbContext.SysUserRoles
            .Where(link => link.UserId == user.Id)
            .ToListAsync();
        var roleIds = links.Select(link => link.RoleId).Distinct().ToArray();
        var roles = await _dbContext.SysRoles
            .Where(role => roleIds.Contains(role.Id))
            .ToListAsync();
        foreach (var link in links)
        {
            link.Role = roles.FirstOrDefault(role => role.Id == link.RoleId)!;
            link.User = user;
        }
        user.SysUserRoles = links;
        return user;
    }

    private async Task<LoginSecurityPolicyOption> GetLoginSecurityPolicyAsync()
    {
        var configString = await _cache.GetValueAsync<string>(
            SysConfigManager.GetLoginSecurityPolicyCacheKey()
        );
        if (!string.IsNullOrWhiteSpace(configString))
        {
            return JsonSerializer.Deserialize(
                    configString,
                    SystemModJsonSerializerContext.Default.LoginSecurityPolicyOption)
                ?? new LoginSecurityPolicyOption();
        }

        return _configuration
                .GetSection(LoginSecurityPolicyOption.ConfigPath)
                .Get<LoginSecurityPolicyOption>()
            ?? new LoginSecurityPolicyOption();
    }
}
