using Entity.ResourceMod;
using EntityFramework.AppDbContext;
using Perigon.AspNetCore.Constants;
using Perigon.PostgreSQL;
using ResourceEntity = Entity.ResourceMod.Resource;

namespace ApiTest;

public sealed class ResourceQueryIsolationTests
{
    private static string ConnectionString =>
        GlobalHooks.App?.GetConnectionStringAsync(AppConst.Default).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("The AppHost PostgreSQL connection is unavailable.");

    [Test]
    public async Task Resource_search_and_favorites_are_paged()
    {
        Guid userId = Guid.CreateVersion7();
        string marker = "query-paging-" + Guid.NewGuid().ToString("N");
        Guid[] resourceIds = [Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7()];
        Guid definitionId = Guid.CreateVersion7();
        Guid environmentId = Guid.CreateVersion7();
        Guid categoryId = Guid.CreateVersion7();

        await using (var context = new DefaultDbContext(ConnectionString))
        {
            await context.ResEnvironments.InsertAsync(new ResEnvironment
            {
                Id = environmentId,
                Name = marker + "-env",
                Color = "#000",
                Icon = "code",
            });
            await context.ResCategories.InsertAsync(new ResCategory
            {
                Id = categoryId,
                Name = marker + "-cat",
                CatalogCode = marker,
                Color = "#000",
            });
            await context.ResDefinitions.InsertAsync(new ResDefinition
            {
                Id = definitionId,
                Name = marker + "-definition",
            });
            await context.Resources.InsertManyReturningAsync(
            [
                new ResourceEntity { Id = resourceIds[0], EnvironmentId = environmentId, CategoryId = categoryId, DefinitionId = definitionId, TagNames = [marker + "-tag-1"] },
                new ResourceEntity { Id = resourceIds[1], EnvironmentId = environmentId, CategoryId = categoryId, DefinitionId = definitionId, TagNames = [marker + "-tag-2"] },
                new ResourceEntity { Id = resourceIds[2], EnvironmentId = environmentId, CategoryId = categoryId, DefinitionId = definitionId, TagNames = [marker + "-tag-3"] },
            ]);
            await context.UserFavoriteResources.InsertManyReturningAsync(
            [
                new UserFavoriteResource { UserId = userId, ResourceId = resourceIds[0] },
                new UserFavoriteResource { UserId = userId, ResourceId = resourceIds[1] },
            ]);
        }

        try
        {
            await using var context = new DefaultDbContext(ConnectionString);
            IQueryable<ResourceEntity> search = context.Resources.Where(resource =>
                context.ResDefinitions.Any(definition =>
                    definition.Id == resource.DefinitionId && definition.Name.Contains(marker))
                || resource.TagNames.Any(tag => tag.Contains(marker)));

            await Assert.That(await search.CountAsync()).IsEqualTo(3);
            List<ResourceEntity> pageRows = await search.OrderBy(resource => resource.Id).Skip(1).Take(1)
                .ToListAsync();
            await Assert.That(pageRows.Count).IsEqualTo(1);
            await Assert.That(resourceIds.Contains(pageRows[0].Id)).IsTrue();

            IQueryable<UserFavoriteResource> favorites = context.UserFavoriteResources
                .Where(favorite => favorite.UserId == userId)
                .Where(favorite => context.Resources.Any(resource => resource.Id == favorite.ResourceId));
            await Assert.That(await favorites.CountAsync()).IsEqualTo(2);
            List<UserFavoriteResource> favoritePage = await favorites.OrderBy(favorite => favorite.Id)
                .Skip(1)
                .Take(1)
                .ToListAsync();
            await Assert.That(favoritePage.Count).IsEqualTo(1);
        }
        finally
        {
            await using var cleanup = new DefaultDbContext(ConnectionString);
            await cleanup.UserFavoriteResources.Where(favorite => resourceIds.Contains(favorite.ResourceId)).ExecuteDeleteAsync();
            await cleanup.Resources.Where(resource => resourceIds.Contains(resource.Id)).ExecuteDeleteAsync();
            await cleanup.ResDefinitions.Where(definition => definition.Id == definitionId).ExecuteDeleteAsync();
            await cleanup.ResCategories.Where(category => category.Id == categoryId).ExecuteDeleteAsync();
            await cleanup.ResEnvironments.Where(environment => environment.Id == environmentId).ExecuteDeleteAsync();
        }
    }
}
