using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;
using Entity.SystemMod;
using NotificationMod.Models.NotificationEventDtos;
using NotificationMod.Models.InAppNotificationDtos;
using NotificationMod.Models.NotificationReceiverDtos;
using NotificationMod.Models.NotificationRuleDtos;
using NotificationMod.Models.NotificationSourceDtos;
using NotificationMod.Models.NotificationDeliveryDtos;
using Perigon.PostgreSQL.Update;

namespace NotificationMod.Managers;

/// <summary>
/// Notification business operations. Configuration and event data are single database;
/// recipient rows are materialized at publish time so each user has an independent read state.
/// </summary>
public sealed class NotificationManager(
    AppDbFactory dbContextFactory,
    IUserContext userContext,
    ILogger<NotificationManager> logger,
    IDataProtectionProvider protectionProvider
) : ManagerBase<DefaultDbContext, InAppNotification>(dbContextFactory, userContext, logger)
{
    private readonly IDataProtector _protector = protectionProvider.CreateProtector("Perigon.NotificationMod.Secrets.v1");
    private static readonly string[] EventTypes = ["pipeline", "push", "tag_push", "merge_request", "event", "notification"];

    private DbSet<T> Set<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
        where T : class, IEntityBase => _dbContext.Set<T>();

    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet.Where(x => x.Id == id).AnyAsync();

    public async Task<PageList<InAppNotificationItemDto>> FilterAsync(InAppNotificationFilterDto filter, bool mine = false)
    {
        IQueryable<InAppNotification> notifications = _dbSet;
        IQueryable<InAppNotificationRecipient> recipients = Set<InAppNotificationRecipient>();
        if (mine)
        {
            var userId = _userContext.UserId;
            var now = DateTimeOffset.UtcNow;
            notifications = notifications.Where(item => item.IsPublished &&
                (!item.ExpiresTime.HasValue || item.ExpiresTime > now) &&
                recipients.Any(row => row.NotificationId == item.Id && row.UserId == userId));
        }
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            notifications = notifications.Where(x => x.Type == filter.Type);
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            notifications = notifications.Where(x => x.Title.Contains(filter.Title!));
        }

        if (filter.Published.HasValue)
        {
            notifications = notifications.Where(x => x.IsPublished == filter.Published.Value);
        }

        int count = await notifications.CountAsync();
        List<InAppNotification> rows = await notifications.OrderByDescending(x => x.CreatedTime)
            .Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync();
        Dictionary<Guid, bool> read = [];
        if (mine && rows.Count > 0)
        {
            Guid[] ids = rows.Select(x => x.Id).ToArray();
            read = (await recipients.Where(x => ids.Contains(x.NotificationId) && x.UserId == _userContext.UserId)
                .ToListAsync()).ToDictionary(x => x.NotificationId, x => x.IsRead);
        }
        return new PageList<InAppNotificationItemDto>
        {
            Count = count,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
            Data = rows.Select(x => ToItem(x, read.GetValueOrDefault(x.Id))).ToList()
        };
    }

    public async Task<InAppNotificationDetailDto?> GetAsync(Guid id)
    {
        InAppNotification? row = await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();
        return row is null ? null : new InAppNotificationDetailDto
        {
            Id = row.Id,
            Type = row.Type,
            Title = row.Title,
            Content = row.Content,
            Link = row.Link,
            IsPublished = row.IsPublished,
            PublishedTime = row.PublishedTime,
            ExpiresTime = row.ExpiresTime,
            CreatedTime = row.CreatedTime,
            UpdatedTime = row.UpdatedTime,
            TargetUserIds = row.TargetUserIds,
        };
    }

    public async Task<InAppNotificationDetailDto> AddAsync(InAppNotificationAddDto dto)
    {
        ValidateContent(dto.Type, dto.Title, dto.Content, dto.Link, dto.ExpiresTime);
        InAppNotification row = new()
        {
            Type = dto.Type.Trim().ToLowerInvariant(),
            Title = dto.Title.Trim(),
            Content = dto.Content,
            Link = dto.Link?.Trim() ?? string.Empty,
            ExpiresTime = dto.ExpiresTime,
            TargetUserIds = dto.TargetUserIds.Distinct().ToArray(),
            CreatedBy = _userContext.UserId,
        };
        await InsertAsync(row);
        if (dto.Publish)
        {
            await PublishAsync(row.Id);
        }

        return (await GetAsync(row.Id))!;
    }

    public async Task<bool> UpdateAsync(Guid id, InAppNotificationUpdateDto dto)
    {
        InAppNotification row = await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync()
            ?? throw NotFound("通知");
        if (row.IsPublished)
        {
            throw new BusinessException("已发布的通知不能修改", StatusCodes.Status409Conflict);
        }

        string type = dto.Type?.Trim().ToLowerInvariant() ?? row.Type;
        string title = dto.Title?.Trim() ?? row.Title;
        string content = dto.Content ?? row.Content;
        string link = dto.Link?.Trim() ?? row.Link;
        DateTimeOffset? expires = dto.ClearExpiresTime ? null : dto.ExpiresTime ?? row.ExpiresTime;
        Guid[] targets = dto.TargetUserIds?.Distinct().ToArray() ?? row.TargetUserIds;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ValidateContent(type, title, content, link, expires);
        int updated = await _dbSet.Where(x => x.Id == id && !x.IsPublished).ExecuteUpdateAsync(set => set
            .Set(x => x.Type, type).Set(x => x.Title, title).Set(x => x.Content, content)
            .Set(x => x.Link, link).Set(x => x.ExpiresTime, expires)
            .Set(x => x.TargetUserIds, targets).Set(x => x.UpdatedTime, now));
        if (updated == 0)
        {
            throw new BusinessException("通知已发布或已删除", StatusCodes.Status409Conflict);
        }
        return true;
    }

    public async Task PublishAsync(Guid id)
    {
        InAppNotification row = await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync()
            ?? throw NotFound("通知");
        if (row.IsPublished)
        {
            return;
        }

        await ExecuteInTransactionAsync(async () =>
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            int claimed = await _dbSet.Where(x => x.Id == id && !x.IsPublished).ExecuteUpdateAsync(set => set
                .Set(x => x.IsPublished, true)
                .Set(x => x.PublishedTime, now)
                .Set(x => x.UpdatedTime, now));
            if (claimed == 0)
            {
                return;
            }

            InAppNotification published = await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync()
                ?? throw NotFound("通知");
            Guid[] requestedUsers = published.TargetUserIds.Distinct().ToArray();
            List<SysUser> users = requestedUsers.Length == 0
                ? await Set<SysUser>().ToListAsync()
                : await Set<SysUser>().Where(x => requestedUsers.Contains(x.Id)).ToListAsync();
            if (requestedUsers.Length > 0 && users.Count != requestedUsers.Length)
            {
                throw new BusinessException("指定的用户不存在", StatusCodes.Status400BadRequest);
            }

            List<InAppNotificationRecipient> existingRows = await Set<InAppNotificationRecipient>()
                .Where(x => x.NotificationId == id).ToListAsync();
            HashSet<Guid> existing = existingRows.Select(x => x.UserId).ToHashSet();
            List<InAppNotificationRecipient> added = users.Where(x => !existing.Contains(x.Id))
                .Select(user => new InAppNotificationRecipient
                {

                    NotificationId = id,
                    UserId = user.Id,
                }).ToList();
            if (added.Count > 0)
            {
                await Set<InAppNotificationRecipient>().BulkInsertAsync(added);
            }
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        InAppNotification? row = await _dbSet.Where(x => x.Id == id && !x.IsPublished).FirstOrDefaultAsync();
        if (row is null)
        {
            throw NotFound("通知");
        }

        await _dbSet.Where(x => x.Id == id).ExecuteUpdateAsync(set => set.Set(x => x.IsDeleted, true));
        return true;
    }

    public Task<PageList<InAppNotificationItemDto>> MineAsync(InAppNotificationFilterDto filter) => FilterAsync(filter, true);

    public async Task<InAppNotificationDetailDto> MineDetailAsync(Guid id)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        IQueryable<InAppNotificationRecipient> recipients = Set<InAppNotificationRecipient>();
        InAppNotification row = await _dbSet.Where(x => x.Id == id && x.IsPublished &&
            (!x.ExpiresTime.HasValue || x.ExpiresTime > now) &&
            recipients.Any(r => r.NotificationId == x.Id && r.UserId == _userContext.UserId))
            .FirstOrDefaultAsync() ?? throw NotFound("通知");
        InAppNotificationRecipient recipient = await Set<InAppNotificationRecipient>()
            .Where(x => x.NotificationId == id && x.UserId == _userContext.UserId).FirstOrDefaultAsync()
            ?? throw NotFound("通知");
        InAppNotificationDetailDto result = ToDetail(row, recipient.IsRead);
        result.TargetUserIds = [];
        return result;
    }

    public async Task<int> UnreadCountAsync()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await Set<InAppNotificationRecipient>().Where(x => x.UserId == _userContext.UserId && !x.IsRead &&
            _dbSet.Any(n => n.Id == x.NotificationId && n.IsPublished &&
                (!n.ExpiresTime.HasValue || n.ExpiresTime > now))).CountAsync();
    }

    public async Task<InAppNotificationReadDto> ReadAsync(Guid id)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (!await _dbSet.Where(x => x.Id == id && x.IsPublished && (!x.ExpiresTime.HasValue || x.ExpiresTime > now)).AnyAsync())
        {
            throw new BusinessException("通知不存在或已过期", StatusCodes.Status404NotFound);
        }

        InAppNotificationRecipient? row = await Set<InAppNotificationRecipient>().Where(x => x.NotificationId == id && x.UserId == _userContext.UserId).FirstOrDefaultAsync()
            ?? throw new BusinessException("通知不存在或未分配给当前用户", StatusCodes.Status404NotFound);
        if (!row.IsRead)
        {
            DateTimeOffset readTime = now;
            await Set<InAppNotificationRecipient>().Where(x => x.Id == row.Id && !x.IsRead)
                .ExecuteUpdateAsync(x => x.Set(y => y.IsRead, true).Set(y => y.ReadTime, readTime));
            row = await Set<InAppNotificationRecipient>().Where(x => x.Id == row.Id).FirstOrDefaultAsync() ?? row;
        }
        return new InAppNotificationReadDto { Id = id, IsRead = true, ReadTime = row.ReadTime };
    }

    public async Task<PageList<NotificationSourceItemDto>> SourcesAsync(FilterBase filter)
    {
        var set = Set<NotificationSource>();
        int count = await set.CountAsync();
        var rows = await set.OrderByDescending(x => x.CreatedTime).Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync();
        return new()
        {
            Count = count,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
            Data = rows.Select(ToItem).ToList()
        };
    }
    public async Task<NotificationSourceItemDto> SaveSourceAsync(Guid? id, NotificationSourceAddDto dto)
    {
        ValidateName(dto.Name);
        ValidateSource(dto);
        NotificationSource row = id is Guid key ? await Set<NotificationSource>().Where(x => x.Id == key).FirstOrDefaultAsync() ?? throw NotFound("来源") : new();
        if (!string.IsNullOrWhiteSpace(dto.Secret))
        {
            row.ProtectedSecret = _protector.Protect(dto.Secret);
        }
        else if (string.IsNullOrWhiteSpace(row.ProtectedSecret))
        {
            throw new BusinessException("Secret不能为空", StatusCodes.Status400BadRequest);
        }

        row.Name = dto.Name.Trim();
        row.Type = dto.Type.Trim().ToLowerInvariant();
        row.Enabled = dto.Enabled;
        row.AuthType = dto.AuthType.Trim().ToLowerInvariant();
        row.HeaderKey = dto.HeaderKey.Trim();
        if (id is null)
        {
            await Set<NotificationSource>().InsertAsync(row);
        }
        else
        {
            await Set<NotificationSource>().Where(x => x.Id == row.Id).ExecuteUpdateAsync(s => s.Set(x => x.Name, row.Name).Set(x => x.Type, row.Type).Set(x => x.Enabled, row.Enabled).Set(x => x.ProtectedSecret, row.ProtectedSecret).Set(x => x.AuthType, row.AuthType).Set(x => x.HeaderKey, row.HeaderKey));
        }

        return ToItem(row);
    }
    public async Task<bool> DeleteSourceAsync(Guid id)
    {
        if (!await Set<NotificationSource>().Where(x => x.Id == id).AnyAsync())
        {
            throw NotFound("来源");
        }

        if (await Set<NotificationRule>().Where(x => x.SourceId == id).AnyAsync())
        {
            throw new BusinessException("来源被通知规则引用，请先解除引用", StatusCodes.Status409Conflict);
        }

        return await Set<NotificationSource>().Where(x => x.Id == id)
            .ExecuteUpdateAsync(set => set.Set(x => x.IsDeleted, true).Set(x => x.Enabled, false)) > 0;
    }

    public async Task<PageList<NotificationReceiverItemDto>> ReceiversAsync(FilterBase filter)
    {
        var set = Set<NotificationReceiver>();
        int count = await set.CountAsync();
        var rows = await set.OrderByDescending(x => x.CreatedTime).Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync();
        return new()
        {
            Count = count,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
            Data = rows.Select(ToItem).ToList()
        };
    }
    public async Task<NotificationReceiverItemDto> SaveReceiverAsync(Guid? id, NotificationReceiverAddDto dto)
    {
        ValidateName(dto.Name);
        ValidateReceiver(dto);
        NotificationReceiver row = id is Guid key ? await Set<NotificationReceiver>().Where(x => x.Id == key).FirstOrDefaultAsync() ?? throw NotFound("接收者") : new();
        if (!string.IsNullOrWhiteSpace(dto.Secret))
        {
            row.ProtectedSecret = _protector.Protect(dto.Secret);
        }
        else if (dto.AuthType.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            row.ProtectedSecret = string.Empty;
        }
        else if (string.IsNullOrWhiteSpace(row.ProtectedSecret))
        {
            throw new BusinessException("Secret不能为空", StatusCodes.Status400BadRequest);
        }

        row.Name = dto.Name.Trim();
        row.Type = dto.Type.Trim().ToLowerInvariant();
        row.Enabled = dto.Enabled;
        row.Endpoint = dto.Endpoint.Trim();
        row.HttpMethod = dto.HttpMethod.Trim().ToUpperInvariant();
        row.AuthType = dto.AuthType.Trim().ToLowerInvariant();
        row.AuthName = dto.AuthName.Trim();
        row.AuthScheme = dto.AuthScheme.Trim();
        row.BodyTemplate = dto.BodyTemplate.Trim();
        row.TimeoutSeconds = dto.TimeoutSeconds;
        row.RateLimitPerMinute = dto.RateLimitPerMinute;
        row.SuccessField = dto.SuccessField.Trim();
        row.SuccessValue = dto.SuccessValue;

        if (id is null)
        {
            await Set<NotificationReceiver>().InsertAsync(row);
        }
        else
        {
            int affected = await Set<NotificationReceiver>().Where(x => x.Id == row.Id)
                .ExecuteUpdateAsync(set => set
                    .Set(x => x.Name, row.Name)
                    .Set(x => x.Type, row.Type)
                    .Set(x => x.Enabled, row.Enabled)
                    .Set(x => x.Endpoint, row.Endpoint)
                    .Set(x => x.HttpMethod, row.HttpMethod)
                    .Set(x => x.AuthType, row.AuthType)
                    .Set(x => x.AuthName, row.AuthName)
                    .Set(x => x.AuthScheme, row.AuthScheme)
                    .Set(x => x.ProtectedSecret, row.ProtectedSecret)
                    .Set(x => x.BodyTemplate, row.BodyTemplate)
                    .Set(x => x.TimeoutSeconds, row.TimeoutSeconds)
                    .Set(x => x.RateLimitPerMinute, row.RateLimitPerMinute)
                    .Set(x => x.SuccessField, row.SuccessField)
                    .Set(x => x.SuccessValue, row.SuccessValue));
            if (affected == 0)
            {
                throw NotFound("接收者");
            }
        }

        return ToItem(row);
    }
    public async Task<bool> DeleteReceiverAsync(Guid id)
    {
        if (!await Set<NotificationReceiver>().Where(x => x.Id == id).AnyAsync())
        {
            throw NotFound("接收者");
        }

        if (await Set<NotificationRuleReceiver>().Where(x => x.ReceiverId == id).AnyAsync())
        {
            throw new BusinessException("接收者被通知规则引用，请先解除引用", StatusCodes.Status409Conflict);
        }

        return await Set<NotificationReceiver>().Where(x => x.Id == id)
            .ExecuteUpdateAsync(set => set.Set(x => x.IsDeleted, true).Set(x => x.Enabled, false)) > 0;
    }

    public async Task<PageList<NotificationRuleItemDto>> RulesAsync(FilterBase filter)
    {
        var set = Set<NotificationRule>();
        int count = await set.CountAsync();
        var rows = await set.OrderByDescending(x => x.CreatedTime).Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync();
        Guid[] ids = rows.Select(x => x.Id).ToArray();
        var links = await Set<NotificationRuleReceiver>().Where(x => ids.Contains(x.RuleId)).ToListAsync();
        return new()
        {
            Count = count,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
            Data = rows.Select(x => ToItem(x, links.Where(l => l.RuleId == x.Id).Select(l => l.ReceiverId).ToArray())).ToList()
        };
    }
    public async Task<NotificationRuleItemDto> SaveRuleAsync(Guid? id, NotificationRuleAddDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.ReceiverIds is not { Length: > 0 and <= 100 } || dto.EventTypes is not { Length: > 0 and <= 6 } || dto.EventTypes.Any(x => !EventTypes.Contains(x)) || dto.Statuses is null || dto.Statuses.Length > 3 || dto.Statuses.Any(x => x is not ("success" or "failed" or "canceled")) || dto.ProjectIds is null || dto.ProjectIds.Length > 100 || dto.ProjectIds.Any(x => x <= 0))
        {
            throw new BusinessException("通知规则参数无效", StatusCodes.Status400BadRequest);
        }

        NotificationSource source = await Set<NotificationSource>().Where(x => x.Id == dto.SourceId).FirstOrDefaultAsync() ?? throw NotFound("来源");
        string[] allowed = source.Type switch
        {
            "gitlab" => ["pipeline", "push", "tag_push", "merge_request"],
            "event" => ["event"],
            "notification" => ["notification"],
            _ => []
        };
        if (dto.EventTypes.Any(x => !allowed.Contains(x)))
        {
            throw new BusinessException("事件类型与来源类型不匹配", StatusCodes.Status400BadRequest);
        }

        Guid[] receiverIds = dto.ReceiverIds.Distinct().ToArray();
        if (await Set<NotificationReceiver>().Where(x => receiverIds.Contains(x.Id)).CountAsync() != receiverIds.Length)
        {
            throw new BusinessException("部分接收者不存在", StatusCodes.Status400BadRequest);
        }

        NotificationRule row = id is Guid key ? await Set<NotificationRule>().Where(x => x.Id == key).FirstOrDefaultAsync() ?? throw NotFound("规则") : new();
        row.Name = dto.Name.Trim();
        row.SourceId = dto.SourceId;
        row.Enabled = dto.Enabled;
        row.EventTypes = dto.EventTypes.Distinct().ToArray();
        row.Statuses = dto.Statuses.Distinct().ToArray();
        row.ProjectIds = dto.ProjectIds.Distinct().ToArray();
        await ExecuteInTransactionAsync(async () => { if (id is null) { await Set<NotificationRule>().InsertAsync(row); } else { await Set<NotificationRule>().Where(x => x.Id == row.Id).ExecuteUpdateAsync(s => s.Set(x => x.Name, row.Name).Set(x => x.SourceId, row.SourceId).Set(x => x.Enabled, row.Enabled).Set(x => x.EventTypes, row.EventTypes).Set(x => x.Statuses, row.Statuses).Set(x => x.ProjectIds, row.ProjectIds)); } await Set<NotificationRuleReceiver>().Where(x => x.RuleId == row.Id).ExecuteDeleteAsync(); await Set<NotificationRuleReceiver>().BulkInsertAsync(receiverIds.Select(receiverId => new NotificationRuleReceiver { RuleId = row.Id, ReceiverId = receiverId })); });
        return ToItem(row, receiverIds);
    }
    public async Task<bool> DeleteRuleAsync(Guid id)
    {
        if (!await Set<NotificationRule>().Where(x => x.Id == id).AnyAsync())
        {
            throw NotFound("规则");
        }

        await ExecuteInTransactionAsync(async () =>
        {
            await Set<NotificationRuleReceiver>().Where(x => x.RuleId == id)
                .ExecuteUpdateAsync(set => set.Set(x => x.IsDeleted, true));
            await Set<NotificationRule>().Where(x => x.Id == id)
                .ExecuteUpdateAsync(set => set.Set(x => x.IsDeleted, true).Set(x => x.Enabled, false));
        });
        return true;
    }

    public async Task<PageList<NotificationEventItemDto>> EventsAsync(FilterBase filter)
    {
        var set = Set<NotificationEvent>();
        int count = await set.CountAsync();
        var rows = await set.OrderByDescending(x => x.CreatedTime).Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync();
        return new()
        {
            Count = count,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
            Data = rows.Select(x => new NotificationEventItemDto { Id = x.Id, EventType = x.EventType, Title = x.Title, Content = x.Content, Status = x.Status, Url = x.Url, OccurredAt = x.OccurredAt, ProcessingStatus = x.ProcessingStatus }).ToList()
        };
    }

    public async Task<PageList<NotificationDeliveryItemDto>> DeliveriesAsync(FilterBase filter)
    {
        var set = Set<NotificationDelivery>();
        int count = await set.CountAsync();
        List<NotificationDelivery> rows = await set.OrderByDescending(x => x.CreatedTime)
            .Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20)).Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync();
        Guid[] eventIds = rows.Select(x => x.EventId).Distinct().ToArray();
        List<NotificationEvent> events = eventIds.Length == 0 ? [] : await Set<NotificationEvent>().Where(x => eventIds.Contains(x.Id)).ToListAsync();
        Dictionary<Guid, NotificationEvent> lookup = events.ToDictionary(x => x.Id);
        return new PageList<NotificationDeliveryItemDto>
        {
            Count = count,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
            Data = rows.Select(row =>
        {
            lookup.TryGetValue(row.EventId, out NotificationEvent? value);
            return new NotificationDeliveryItemDto { Id = row.Id, EventId = row.EventId, EventType = value?.EventType ?? string.Empty, ProjectName = value?.ProjectName ?? string.Empty, ObjectId = value?.ObjectId ?? string.Empty, ReceiverName = row.ReceiverName, Status = row.Status, Error = row.Error, SentAt = row.SentAt };
        }).ToList()
        };
    }

    private async Task<bool> DeleteSetAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Guid id, string name) where T : class, IEntityBase, new()
    {
        if (await Set<T>().Where(x => x.Id == id).ExecuteDeleteAsync() == 0)
        {
            throw NotFound(name);
        }

        return true;
    }
    private static InAppNotificationItemDto ToItem(InAppNotification x, bool read) => new() { Id = x.Id, Type = x.Type, Title = x.Title, Content = x.Content, Link = x.Link, IsPublished = x.IsPublished, PublishedTime = x.PublishedTime, ExpiresTime = x.ExpiresTime, IsRead = read, CreatedTime = x.CreatedTime };
    private static InAppNotificationDetailDto ToDetail(InAppNotification x, bool read) => new() { Id = x.Id, Type = x.Type, Title = x.Title, Content = x.Content, Link = x.Link, IsPublished = x.IsPublished, PublishedTime = x.PublishedTime, ExpiresTime = x.ExpiresTime, IsRead = read, CreatedTime = x.CreatedTime, UpdatedTime = x.UpdatedTime, TargetUserIds = x.TargetUserIds };
    private static NotificationSourceItemDto ToItem(NotificationSource x) => new() { Id = x.Id, Name = x.Name, Type = x.Type, Enabled = x.Enabled, HasSecret = !string.IsNullOrWhiteSpace(x.ProtectedSecret), AuthType = x.AuthType, HeaderKey = x.HeaderKey };
    private static NotificationReceiverItemDto ToItem(NotificationReceiver x) => new() { Id = x.Id, Name = x.Name, Type = x.Type, Enabled = x.Enabled, HasSecret = !string.IsNullOrWhiteSpace(x.ProtectedSecret), Endpoint = x.Endpoint, HttpMethod = x.HttpMethod, AuthType = x.AuthType, AuthName = x.AuthName, AuthScheme = x.AuthScheme, BodyTemplate = x.BodyTemplate, TimeoutSeconds = x.TimeoutSeconds, RateLimitPerMinute = x.RateLimitPerMinute, SuccessField = x.SuccessField, SuccessValue = x.SuccessValue };
    private static NotificationRuleItemDto ToItem(NotificationRule x, Guid[] receiverIds) => new() { Id = x.Id, Name = x.Name, SourceId = x.SourceId, ReceiverIds = receiverIds, EventTypes = x.EventTypes, Statuses = x.Statuses, ProjectIds = x.ProjectIds, Enabled = x.Enabled };
    private static BusinessException NotFound(string name) => new($"{name}不存在", StatusCodes.Status404NotFound);
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
        {
            throw new BusinessException("名称必须为1至100个字符", 400);
        }
    }
    private static void ValidateContent(string type, string title, string content, string link, DateTimeOffset? expires)
    {
        if (type is not ("message" or "announcement") || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            throw new BusinessException("站内通知类型、标题和内容无效", 400);
        }

        if (!string.IsNullOrWhiteSpace(link) && (!Uri.TryCreate(link, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")))
        {
            throw new BusinessException("链接必须为HTTP(S)绝对地址", 400);
        }

        if (expires <= DateTimeOffset.UtcNow)
        {
            throw new BusinessException("过期时间必须晚于当前时间", 400);
        }
    }
    private static void ValidateSource(NotificationSourceAddDto dto)
    {
        if (dto.Type is not ("gitlab" or "event" or "notification") || dto.AuthType is not ("query" or "header") || (dto.AuthType == "header" && !Regex.IsMatch(dto.HeaderKey, "^[A-Za-z0-9-]{1,100}$")))
        {
            throw new BusinessException("来源参数无效", 400);
        }
    }
    private static void ValidateReceiver(NotificationReceiverAddDto dto)
    {
        if (!Uri.TryCreate(dto.Endpoint, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https") || dto.HttpMethod is not ("POST" or "PUT") || dto.AuthType is not ("query" or "header" or "none") || dto.TimeoutSeconds is < 1 or > 30 || dto.RateLimitPerMinute is < 1 or > 600)
        {
            throw new BusinessException("接收者参数无效", 400);
        }

        try
        {
            using var _ = JsonDocument.Parse(dto.BodyTemplate.Replace("{content}", "content").Replace("{title}", "title").Replace("{link}", "https://example.com"));
        }
        catch (JsonException) { throw new BusinessException("请求体模板必须是合法JSON", 400); }
    }
}

