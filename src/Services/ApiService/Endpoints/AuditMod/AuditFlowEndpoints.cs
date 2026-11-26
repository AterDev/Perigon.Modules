using AuditMod.Managers;
using AuditMod.Demands;
using AuditMod.Models.AuditFlowDtos;
using Perigon.AspNetCore.Models;
using Share;
using Share.Implement;

namespace ApiService.Endpoints;

public sealed class AuditFlowEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/admin/audit-flows", "AuditFlow", WebConst.AdminUser)
            .RequireAuthorization(WebConst.User);
        group.MapGet("", FilterAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("", AddAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapPatch("/{id:guid}/enabled", SetEnabledAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    public static Task<PageList<AuditFlowItemDto>> FilterAsync(
        [AsParameters] AuditFlowFilterDto filter,
        AuditFlowManager manager,
        CancellationToken cancellationToken) => manager.FilterAsync(filter, cancellationToken);

    public static async Task<RestEndpointBase.ApiResult<AuditFlowDetailDto>> GetAsync(
        Guid id,
        AuditFlowManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id, cancellationToken);
        return result is null ? NotFound(Localizer.NotFoundResource) : result;
    }

    public static async Task<RestEndpointBase.ApiResult<AuditFlowDetailDto>> AddAsync(
        AuditFlowAddDto dto,
        AuditFlowManager manager,
        IAuditUserDemand users,
        CancellationToken cancellationToken)
    {
        var userIds = await GetAvailableUserIdsAsync(dto.Steps, users, cancellationToken);
        var result = await manager.AddAsync(dto, userIds, cancellationToken);
        return TypedResults.Created($"/api/admin/audit-flows/{result.Id}", result);
    }

    public static async Task<AuditFlowDetailDto> UpdateAsync(
        Guid id,
        AuditFlowUpdateDto dto,
        AuditFlowManager manager,
        IAuditUserDemand users,
        CancellationToken cancellationToken)
    {
        var userIds = await GetAvailableUserIdsAsync(dto.Steps, users, cancellationToken);
        return await manager.UpdateAsync(id, dto, userIds, cancellationToken);
    }

    private static async Task<HashSet<Guid>> GetAvailableUserIdsAsync(
        IEnumerable<AuditFlowStepDto> steps,
        IAuditUserDemand users,
        CancellationToken cancellationToken)
    {
        var requestedIds = steps.SelectMany(step => step.AuditorUserIds).Distinct().ToArray();
        return requestedIds.Length == 0
            ? []
            : (await users.GetExistingUserIdsAsync(requestedIds, cancellationToken)).ToHashSet();
    }

    public static async Task<RestEndpointBase.ApiResult<bool>> SetEnabledAsync(
        Guid id,
        AuditFlowEnabledUpdateDto dto,
        AuditFlowManager manager,
        CancellationToken cancellationToken)
    {
        await manager.SetEnabledAsync(id, dto, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<RestEndpointBase.ApiResult<bool>> DeleteAsync(
        Guid id,
        AuditFlowManager manager,
        CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
