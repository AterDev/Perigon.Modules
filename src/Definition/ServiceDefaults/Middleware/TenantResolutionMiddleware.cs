using Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Perigon.AspNetCore.Abstraction;
using Perigon.AspNetCore.Services;

namespace ServiceDefaults.Middleware;

/// <summary>
/// Middleware to resolve tenant metadata and cache it in memory.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUserContext userContext,
        DefaultDbContext dbContext,
        CacheService cache
    )
    {
        try
        {
            if (userContext.TenantId == Guid.Empty)
            {
                if (context.User.Identity?.IsAuthenticated != true)
                {
                    await _next(context);
                    return;
                }

                _logger.LogWarning("Authenticated user has no TenantId claim");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            var cacheKey = $"{WebConst.TenantId}__{userContext.TenantId}";
            var tenant = cache.GetMemory<Tenant>(cacheKey);

            if (tenant is null)
            {
                tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        t => t.Id == userContext.TenantId && !t.Disabled,
                        context.RequestAborted
                    );

                if (tenant is not null)
                {
                    cache.SetMemory(cacheKey, tenant, TimeSpan.FromDays(1));
                    _logger.LogInformation(
                        "Tenant {TenantId} loaded from database and cached",
                        userContext.TenantId
                    );
                }
            }
            else
            {
                _logger.LogDebug("Tenant {TenantId} loaded from memory cache", userContext.TenantId);
            }

            if (tenant is not null && (!tenant.Disabled && !tenant.IsDeleted))
            {
                userContext.TenantType = tenant.Type.ToString();
            }
            else
            {
                _logger.LogWarning(
                    "Tenant {TenantId} not found; rejecting the authenticated request",
                    userContext.TenantId
                );
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant connection strings");
            throw;
        }

        await _next(context);
    }
}
