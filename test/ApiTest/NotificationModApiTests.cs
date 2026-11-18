using ApiTest.Data;
using Entity.NotificationMod;
using EntityFramework.AppDbContext;
using NotificationMod.Models.InAppNotificationDtos;
using Perigon.AspNetCore.Constants;
using Perigon.AspNetCore.Models;
using Perigon.PostgreSQL;
using System.Net;
using System.Net.Http.Json;

namespace ApiTest;

/// <summary>
/// Public HTTP contract tests for in-app notifications. Database access is
/// used only to expire and clean up rows because the public API intentionally
/// does not expose a clock-control or published-message delete operation.
/// </summary>
public sealed class NotificationModApiTests
{
    private static string ConnectionString =>
        GlobalHooks.App?.GetConnectionStringAsync(AppConst.Default).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("The AppHost PostgreSQL connection is unavailable.");

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task PublishedNotifications_ShouldKeepReadStatePerUserAndRespectTargetUsers(TestHttpClientData data)
    {
        HttpClient admin = data.HttpClient;
        (HttpClient userA, Guid userAId) = await data.CreateSysUserClientAsync();
        (HttpClient userB, Guid userBId) = await data.CreateSysUserClientAsync();
        List<Guid> notificationIds = [];
        string sharedTitle = $"api-shared-{Guid.CreateVersion7():N}";
        string targetedTitle = $"api-targeted-{Guid.CreateVersion7():N}";

        try
        {
            InAppNotificationDetailDto shared = await AddAsync(admin, new InAppNotificationAddDto
            {
                Type = "announcement",
                Title = sharedTitle,
                Content = "shared-content",
                TargetUserIds = [userAId, userBId],
                Publish = true
            });
            notificationIds.Add(shared.Id);

            PageList<InAppNotificationItemDto> beforeA = await MineAsync(userA, sharedTitle);
            PageList<InAppNotificationItemDto> beforeB = await MineAsync(userB, sharedTitle);
            await Assert.That(beforeA.Count).IsEqualTo(1);
            await Assert.That(beforeB.Count).IsEqualTo(1);
            await Assert.That(beforeA.Data[0].IsRead).IsFalse();
            await Assert.That(beforeB.Data[0].IsRead).IsFalse();

            InAppNotificationReadDto read = await PostAsync<InAppNotificationReadDto>(
                userA,
                $"/api/Notification/{shared.Id}/read",
                new
                {
                },
                HttpStatusCode.OK);
            await Assert.That(read.Id).IsEqualTo(shared.Id);
            await Assert.That(read.IsRead).IsTrue();

            InAppNotificationDetailDto detailA = await GetAsync<InAppNotificationDetailDto>(
                userA,
                $"/api/Notification/mine/{shared.Id}",
                HttpStatusCode.OK);
            InAppNotificationDetailDto detailB = await GetAsync<InAppNotificationDetailDto>(
                userB,
                $"/api/Notification/mine/{shared.Id}",
                HttpStatusCode.OK);
            await Assert.That(detailA.IsRead).IsTrue();
            await Assert.That(detailB.IsRead).IsFalse();

            InAppNotificationDetailDto targeted = await AddAsync(admin, new InAppNotificationAddDto
            {
                Type = "message",
                Title = targetedTitle,
                Content = "targeted-content",
                TargetUserIds = [userAId],
                Publish = true
            });
            notificationIds.Add(targeted.Id);

            PageList<InAppNotificationItemDto> targetedA = await MineAsync(userA, targetedTitle);
            PageList<InAppNotificationItemDto> targetedB = await MineAsync(userB, targetedTitle);
            await Assert.That(targetedA.Data.Select(item => item.Id)).IsEquivalentTo([targeted.Id]);
            await Assert.That(targetedB.Count).IsEqualTo(0);
            await Assert.That(targetedB.Data.Count).IsEqualTo(0);
        }
        finally
        {
            await SoftDeleteAsync(notificationIds);
        }
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task RepeatedPublish_ShouldKeepOneRecipientAndSnapshotBroadcastAudience(TestHttpClientData data)
    {
        HttpClient admin = data.HttpClient;
        (HttpClient existingUser, _) = await data.CreateSysUserClientAsync();
        Guid notificationId = Guid.Empty;
        string title = $"api-snapshot-{Guid.CreateVersion7():N}";

        try
        {
            InAppNotificationDetailDto created = await AddAsync(admin, new InAppNotificationAddDto
            {
                Type = "announcement",
                Title = title,
                Content = "snapshot-content",
                Publish = false
            });
            notificationId = created.Id;

            using (HttpResponseMessage firstPublish = await admin.PostAsync(
                $"/api/Notification/{notificationId}/publish",
                new StringContent(string.Empty)))
            {
                await Assert.That(firstPublish.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            }

            using (HttpResponseMessage secondPublish = await admin.PostAsync(
                $"/api/Notification/{notificationId}/publish",
                new StringContent(string.Empty)))
            {
                await Assert.That(secondPublish.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            }

            PageList<InAppNotificationItemDto> existingRows = await MineAsync(existingUser, title);
            await Assert.That(existingRows.Count).IsEqualTo(1);

            await using (DefaultDbContext context = new(ConnectionString))
            {
                List<InAppNotificationRecipient> recipients = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationId)
                    .ToListAsync();
                await Assert.That(recipients.Count).IsEqualTo(recipients.Select(item => item.UserId).Distinct().Count());
            }

            (HttpClient newUser, _) = await data.CreateSysUserClientAsync();
            PageList<InAppNotificationItemDto> newUserRows = await MineAsync(newUser, title);
            await Assert.That(newUserRows.Count).IsEqualTo(0);
        }
        finally
        {
            await SoftDeleteAsync([notificationId]);
        }
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task UnpublishedOrExpiredNotifications_ShouldNotBeVisibleOrReadable(TestHttpClientData data)
    {
        HttpClient admin = data.HttpClient;
        (HttpClient user, Guid userId) = await data.CreateSysUserClientAsync();
        List<Guid> notificationIds = [];
        string unpublishedTitle = $"api-unpublished-{Guid.CreateVersion7():N}";
        string expiredTitle = $"api-expired-{Guid.CreateVersion7():N}";

        try
        {
            InAppNotificationDetailDto unpublished = await AddAsync(admin, new InAppNotificationAddDto
            {
                Type = "message",
                Title = unpublishedTitle,
                Content = "unpublished-content",
                TargetUserIds = [userId],
                Publish = false
            });
            notificationIds.Add(unpublished.Id);

            PageList<InAppNotificationItemDto> unpublishedRows = await MineAsync(user, unpublishedTitle);
            await Assert.That(unpublishedRows.Count).IsEqualTo(0);
            await ExpectStatusAsync(user, $"/api/Notification/mine/{unpublished.Id}", HttpMethod.Get, HttpStatusCode.NotFound);
            await ExpectStatusAsync(user, $"/api/Notification/{unpublished.Id}/read", HttpMethod.Post, HttpStatusCode.NotFound);

            InAppNotificationDetailDto expired = await AddAsync(admin, new InAppNotificationAddDto
            {
                Type = "announcement",
                Title = expiredTitle,
                Content = "expired-content",
                ExpiresTime = DateTimeOffset.UtcNow.AddMinutes(5),
                TargetUserIds = [userId],
                Publish = true
            });
            notificationIds.Add(expired.Id);

            await using (DefaultDbContext context = new(ConnectionString))
            {
                await context.InAppNotifications.Where(item => item.Id == expired.Id)
                    .ExecuteUpdateAsync(update => update.Set(item => item.ExpiresTime, DateTimeOffset.UtcNow.AddMinutes(-1)));
            }

            PageList<InAppNotificationItemDto> expiredRows = await MineAsync(user, expiredTitle);
            await Assert.That(expiredRows.Count).IsEqualTo(0);
            await ExpectStatusAsync(user, $"/api/Notification/mine/{expired.Id}", HttpMethod.Get, HttpStatusCode.NotFound);
            await ExpectStatusAsync(user, $"/api/Notification/{expired.Id}/read", HttpMethod.Post, HttpStatusCode.NotFound);
        }
        finally
        {
            await SoftDeleteAsync(notificationIds);
        }
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task OrdinaryUser_ShouldNotManageNotifications(TestHttpClientData data)
    {
        (HttpClient user, _) = await data.CreateSysUserClientAsync();

        using (HttpResponseMessage add = await user.PostAsJsonAsync(
            "/api/Notification",
            new InAppNotificationAddDto
            {
                Type = "message",
                Title = $"ordinary-user-{Guid.CreateVersion7():N}",
                Content = "must-be-forbidden"
            }))
        {
            await Assert.That(add.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        }

        using (HttpResponseMessage filter = await user.PostAsJsonAsync(
            "/api/Notification/filter",
            new InAppNotificationFilterDto { PageIndex = 1, PageSize = 10 }))
        {
            await Assert.That(filter.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        }
    }

    private static async Task<InAppNotificationDetailDto> AddAsync(HttpClient client, InAppNotificationAddDto dto) =>
        await PostAsync<InAppNotificationDetailDto>(client, "/api/Notification", dto, HttpStatusCode.Created);

    private static async Task<PageList<InAppNotificationItemDto>> MineAsync(HttpClient client, string title) =>
        await PostAsync<PageList<InAppNotificationItemDto>>(
            client,
            "/api/Notification/mine",
            new InAppNotificationFilterDto { Title = title, PageIndex = 1, PageSize = 100 },
            HttpStatusCode.OK);

    private static async Task<T> PostAsync<T>(HttpClient client, string path, object body, HttpStatusCode expected)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(path, body);
        await Assert.That(response.StatusCode).IsEqualTo(expected);
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"{path} returned an empty response.");
    }

    private static async Task<T> GetAsync<T>(HttpClient client, string path, HttpStatusCode expected)
    {
        using HttpResponseMessage response = await client.GetAsync(path);
        await Assert.That(response.StatusCode).IsEqualTo(expected);
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"{path} returned an empty response.");
    }

    private static async Task ExpectStatusAsync(HttpClient client, string path, HttpMethod method, HttpStatusCode expected)
    {
        using HttpRequestMessage request = new(method, path);
        using HttpResponseMessage response = await client.SendAsync(request);
        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    private static async Task SoftDeleteAsync(IReadOnlyCollection<Guid> ids)
    {
        Guid[] actualIds = ids.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (actualIds.Length == 0)
        {
            return;
        }

        await using DefaultDbContext context = new(ConnectionString);
        await context.InAppNotificationRecipients.Where(item => actualIds.Contains(item.NotificationId))
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.InAppNotifications.Where(item => actualIds.Contains(item.Id))
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
    }
}
