using Entity.ResourceMod;
using EntityFramework.AppDbContext;
using Perigon.AspNetCore.Constants;
using Perigon.PostgreSQL;
using System.Net;
using ResourceEntity = Entity.ResourceMod.Resource;

namespace ApiTest;

/// <summary>
/// Verifies that the SQL-backed resource search and favorite queries keep
/// tenant filters while applying Count/Skip/Take in PostgreSQL.
/// </summary>
public sealed class ResourceQueryIsolationTests
{
    private static string ConnectionString =>
        GlobalHooks.App?.GetConnectionStringAsync(AppConst.Default).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("The AppHost PostgreSQL connection is unavailable.");

    [Before(Class)]
    public static async Task WaitForPostgreSqlAsync()
    {
        if (GlobalHooks.NotificationService is not null)
        {
            await GlobalHooks.NotificationService
                .WaitForResourceAsync("ApiService", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(60));
        }
    }

    [Test]
    public async Task Resource_search_and_favorites_are_paged_and_isolated_by_tenant()
    {
        Guid tenantA = Guid.CreateVersion7();
        Guid tenantB = Guid.CreateVersion7();
        Guid userId = Guid.CreateVersion7();
        string marker = "query-isolation-" + Guid.NewGuid().ToString("N");
        Guid[] resourceIds = [Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7()];
        Guid definitionA = Guid.CreateVersion7();
        Guid definitionB = Guid.CreateVersion7();
        Guid environmentA = Guid.CreateVersion7();
        Guid environmentB = Guid.CreateVersion7();
        Guid categoryA = Guid.CreateVersion7();
        Guid categoryB = Guid.CreateVersion7();

        await using (var catalog = new DefaultDbContext(ConnectionString))
        {
            await catalog.ResEnvironments.InsertManyReturningAsync(
            [
                new ResEnvironment { Id = environmentA, TenantId = tenantA, Name = marker + "-env-a", Color = "#000", Icon = "a" },
                new ResEnvironment { Id = environmentB, TenantId = tenantB, Name = marker + "-env-b", Color = "#000", Icon = "b" }
            ]);
            await catalog.ResCategories.InsertManyReturningAsync(
            [
                new ResCategory { Id = categoryA, TenantId = tenantA, Name = marker + "-cat-a", CatalogCode = marker + "-a", Color = "#000" },
                new ResCategory { Id = categoryB, TenantId = tenantB, Name = marker + "-cat-b", CatalogCode = marker + "-b", Color = "#000" }
            ]);
            await catalog.ResDefinitions.InsertManyReturningAsync(
            [
                new ResDefinition { Id = definitionA, TenantId = tenantA, Name = marker + "-definition" },
                new ResDefinition { Id = definitionB, TenantId = tenantB, Name = marker + "-definition" }
            ]);
            await catalog.Resources.InsertManyReturningAsync(
            [
                new ResourceEntity { Id = resourceIds[0], TenantId = tenantA, EnvironmentId = environmentA, CategoryId = categoryA, DefinitionId = definitionA, TagNames = [marker + "-tag-a1"] },
                new ResourceEntity { Id = resourceIds[1], TenantId = tenantA, EnvironmentId = environmentA, CategoryId = categoryA, DefinitionId = definitionA, TagNames = ["ordinary-a2"] },
                new ResourceEntity { Id = resourceIds[2], TenantId = tenantA, EnvironmentId = environmentA, CategoryId = categoryA, DefinitionId = definitionA, TagNames = [marker + "-tag-a3"] },
                new ResourceEntity { Id = resourceIds[3], TenantId = tenantB, EnvironmentId = environmentB, CategoryId = categoryB, DefinitionId = definitionB, TagNames = [marker + "-tag-b1"] }
            ]);
            await catalog.UserFavoriteResources.InsertManyReturningAsync(
            [
                new UserFavoriteResource { UserId = userId, TenantId = tenantA, ResourceId = resourceIds[0] },
                new UserFavoriteResource { UserId = userId, TenantId = tenantA, ResourceId = resourceIds[1] },
                new UserFavoriteResource { UserId = userId, TenantId = tenantB, ResourceId = resourceIds[3] }
            ]);
        }

        try
        {
            await using var contextA = new DefaultDbContext(ConnectionString, tenantA);
            await using var contextB = new DefaultDbContext(ConnectionString, tenantB);

            IQueryable<ResourceEntity> searchA = contextA.Resources.Where(resource =>
                contextA.ResDefinitions.Any(definition =>
                    definition.Id == resource.DefinitionId && definition.Name.Contains(marker))
                || resource.TagNames.Any(tag => tag.Contains(marker)));
            IQueryable<ResourceEntity> searchB = contextB.Resources.Where(resource =>
                contextB.ResDefinitions.Any(definition =>
                    definition.Id == resource.DefinitionId && definition.Name.Contains(marker))
                || resource.TagNames.Any(tag => tag.Contains(marker)));

            await Assert.That(await searchA.CountAsync()).IsEqualTo(3);
            await Assert.That(await searchB.CountAsync()).IsEqualTo(1);
            List<ResourceEntity> pageRows = await searchA.OrderBy(resource => resource.Id).Skip(1).Take(1)
                .ToListAsync();
            Guid[] pageA = pageRows.Select(resource => resource.Id).ToArray();
            await Assert.That(pageA.Length).IsEqualTo(1);
            await Assert.That(resourceIds[..3].Contains(pageA[0])).IsTrue();

            IQueryable<UserFavoriteResource> favoritesA = contextA.UserFavoriteResources
                .Where(favorite => favorite.UserId == userId)
                .Where(favorite => contextA.Resources.Any(resource => resource.Id == favorite.ResourceId));
            IQueryable<UserFavoriteResource> favoritesB = contextB.UserFavoriteResources
                .Where(favorite => favorite.UserId == userId)
                .Where(favorite => contextB.Resources.Any(resource => resource.Id == favorite.ResourceId));

            await Assert.That(await favoritesA.CountAsync()).IsEqualTo(2);
            await Assert.That(await favoritesB.CountAsync()).IsEqualTo(1);
            List<UserFavoriteResource> favoriteRows = await favoritesA.OrderBy(favorite => favorite.Id).Skip(1).Take(1)
                .ToListAsync();
            Guid[] favoritePage = favoriteRows.Select(favorite => favorite.ResourceId).ToArray();
            await Assert.That(favoritePage.Length).IsEqualTo(1);
            await Assert.That(resourceIds[..3].Contains(favoritePage[0])).IsTrue();
        }
        finally
        {
            await using var cleanupA = new DefaultDbContext(ConnectionString, tenantA);
            await cleanupA.UserFavoriteResources.Where(favorite => resourceIds.Contains(favorite.ResourceId)).ExecuteDeleteAsync();
            await cleanupA.Resources.Where(resource => resourceIds.Contains(resource.Id)).ExecuteDeleteAsync();
            await cleanupA.ResDefinitions.Where(definition => definition.Id == definitionA).ExecuteDeleteAsync();
            await cleanupA.ResCategories.Where(category => category.Id == categoryA).ExecuteDeleteAsync();
            await cleanupA.ResEnvironments.Where(environment => environment.Id == environmentA).ExecuteDeleteAsync();

            await using var cleanupB = new DefaultDbContext(ConnectionString, tenantB);
            await cleanupB.UserFavoriteResources.Where(favorite => resourceIds.Contains(favorite.ResourceId)).ExecuteDeleteAsync();
            await cleanupB.Resources.Where(resource => resourceIds.Contains(resource.Id)).ExecuteDeleteAsync();
            await cleanupB.ResDefinitions.Where(definition => definition.Id == definitionB).ExecuteDeleteAsync();
            await cleanupB.ResCategories.Where(category => category.Id == categoryB).ExecuteDeleteAsync();
            await cleanupB.ResEnvironments.Where(environment => environment.Id == environmentB).ExecuteDeleteAsync();
        }
    }
}
