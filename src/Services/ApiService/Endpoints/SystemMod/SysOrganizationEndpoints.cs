using Microsoft.AspNetCore.Http.HttpResults;
using Perigon.AspNetCore.Models;
using SystemMod.Managers;
using SystemMod.Models.SysOrganizationDtos;

namespace ApiService.Endpoints.SystemMod;

/// <summary>
/// 组织结构管理接口。
/// </summary>
public sealed class SysOrganizationEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysOrganization", "SysOrganization", WebConst.AdminUser);
        group.MapGet("", ListTreeAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    private static Task<List<SysOrganizationItemDto>> ListTreeAsync(
        SysOrganizationManager manager,
        CancellationToken cancellationToken) => manager.ListTreeAsync(cancellationToken);

    private static async Task<Created<SysOrganizationItemDto>> AddAsync(
        SysOrganizationAddDto dto,
        SysOrganizationManager manager,
        CancellationToken cancellationToken)
    {
        SysOrganizationItemDto organization = await manager.AddAsync(dto, cancellationToken);
        return TypedResults.Created($"/api/SysOrganization/{organization.Id}", organization);
    }

    private static async Task<Ok<SysOrganizationItemDto>> UpdateAsync(
        Guid id,
        SysOrganizationUpdateDto dto,
        SysOrganizationManager manager,
        CancellationToken cancellationToken)
    {
        SysOrganizationItemDto organization = await manager.UpdateAsync(id, dto, cancellationToken);
        return TypedResults.Ok(organization);
    }

    private static async Task<NoContent> DeleteAsync(
        Guid id,
        SysOrganizationManager manager,
        CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
