using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;
using NotificationMod.Models.WebhookDtos;

namespace NotificationMod.Services;

/// <summary>Converts GitLab and generic webhook payloads to the stable event contract used by rules.</summary>
public sealed class SourcePayloadParser
{
    public NormalizedEventDto Parse(JsonElement payload, string sourceType, Guid sourceId, string? headerEventName, string? headerEventKey)
    {
        string eventType = NormalizeEventType(headerEventName, FirstString(payload, "event_type", "eventName", "object_kind"));
        _ = payload.GetRawText();
        string projectName = FirstString(payload, "project.name", "repository.name", "project.path_with_namespace") ?? string.Empty;
        string nameSpace = FirstString(payload, "project.namespace", "project.path_with_namespace", "repository.git_http_url") ?? string.Empty;
        string status = FirstString(payload, "status", "build_status", "object_attributes.status", "object_attributes.state", "pipeline.status", "build.status") ?? string.Empty;
        string title = FirstString(payload, "title", "object_attributes.title", "commit.title", "project.name") ?? string.Empty;
        string actor = FirstString(payload, "user_name", "user.username", "user_username", "user.name", "author.username") ?? string.Empty;
        string url = FirstString(payload, "project.web_url", "object_attributes.url", "commit.url", "repository.homepage") ?? string.Empty;
        long projectId = FirstLong(payload, "project.id", "project_id");
        string objectId = FirstString(payload, "object_attributes.id", "commit.id", "build_id", "pipeline.id") ?? string.Empty;
        string action = FirstString(payload, "action", "object_attributes.action", "event_name") ?? string.Empty;
        string fullReference = FirstString(payload, "ref", "ref_name", "object_attributes.target_branch") ?? string.Empty;
        string reference = fullReference.StartsWith("refs/heads/", StringComparison.Ordinal) ? fullReference["refs/heads/".Length..] : fullReference.StartsWith("refs/tags/", StringComparison.Ordinal) ? fullReference["refs/tags/".Length..] : fullReference;
        string sha = FirstString(payload, "checkout_sha", "after", "sha", "commit.id") ?? string.Empty;
        DateTimeOffset? occurred = FirstDate(payload, "occurred_at", "created_at", "timestamp", "object_attributes.created_at");
        DateTimeOffset? finished = FirstDate(payload, "finished_at", "object_attributes.finished_at", "build_finished_at");
        double? duration = FirstDouble(payload, "duration", "build_duration");
        bool tag = eventType.Equals("tag_push", StringComparison.OrdinalIgnoreCase) || reference.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase);
        string content = FirstString(payload, "content") ?? $"{eventType}: {title}; project={projectName}; status={status}; actor={actor}; ref={reference}";
        if (sourceType.Equals("gitlab", StringComparison.OrdinalIgnoreCase) && eventType is not ("pipeline" or "push" or "tag_push" or "merge_request"))
            throw new BusinessException("不支持的GitLab事件类型", StatusCodes.Status400BadRequest);
        if (sourceType.Equals("gitlab", StringComparison.OrdinalIgnoreCase) && (projectId <= 0 || string.IsNullOrWhiteSpace(projectName)))
            throw new BusinessException("GitLab项目字段无效", StatusCodes.Status400BadRequest);
        if (sourceType.Equals("gitlab", StringComparison.OrdinalIgnoreCase) && eventType == "pipeline" && (string.IsNullOrWhiteSpace(objectId) || string.IsNullOrWhiteSpace(status)))
            throw new BusinessException("流水线字段无效", StatusCodes.Status400BadRequest);
        if (sourceType.Equals("gitlab", StringComparison.OrdinalIgnoreCase) && eventType is "push" or "tag_push" && string.IsNullOrWhiteSpace(reference))
            throw new BusinessException("推送引用字段无效", StatusCodes.Status400BadRequest);
        if (sourceType.Equals("gitlab", StringComparison.OrdinalIgnoreCase) && eventType == "merge_request" && (string.IsNullOrWhiteSpace(objectId) || string.IsNullOrWhiteSpace(title)))
            throw new BusinessException("合并请求字段无效", StatusCodes.Status400BadRequest);
        if (eventType == "merge_request") status = status switch { "merged" => "success", "closed" => "canceled", _ => string.Empty };
        if (sourceType.Equals("event", StringComparison.OrdinalIgnoreCase))
        {
            eventType = "event";
            if (string.IsNullOrWhiteSpace(title)) throw new BusinessException("事件标题无效", StatusCodes.Status400BadRequest);
        }
        if (sourceType.Equals("notification", StringComparison.OrdinalIgnoreCase))
        {
            eventType = "notification";
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(FirstString(payload, "content"))) throw new BusinessException("通知内容无效", StatusCodes.Status400BadRequest);
        }
        string eventKey = BuildEventKey(sourceType, sourceId, headerEventKey, eventType, projectId, objectId, status, action, fullReference, sha, title, content, occurred, finished);
        return new NormalizedEventDto
        {
            EventKey = eventKey, EventType = eventType, ProjectId = projectId, ObjectId = Limit(objectId, 100),
            ProjectName = Limit(projectName, 300), Namespace = Limit(nameSpace, 500), ActorName = Limit(actor, 300),
            Title = Limit(title, 500), Content = Limit(content, 8000), Status = Limit(status, 40), Action = Limit(action, 50),
            Ref = Limit(reference, 300), Tag = tag, Sha = Limit(sha, 64), OccurredAt = occurred, FinishedAt = finished, Duration = duration, Url = Limit(url, 1200),
        };
    }

    private static string BuildEventKey(string sourceType, Guid sourceId, string? headerKey, string eventType, long projectId, string objectId, string status, string action, string reference, string sha, string title, string content, DateTimeOffset? occurred, DateTimeOffset? finished)
    {
        string material = !string.IsNullOrWhiteSpace(headerKey) ? $"{sourceId}:{headerKey}" : sourceType.ToLowerInvariant() switch
        {
            "gitlab" when eventType == "pipeline" => $"{sourceId}:pipeline:{projectId}:{objectId}:{status}:{(finished ?? occurred)?.ToUniversalTime():O}",
            "gitlab" when eventType is "push" or "tag_push" => $"{sourceId}:{eventType}:{projectId}:{sha}:{reference}",
            "gitlab" when eventType == "merge_request" => $"{sourceId}:merge_request:{projectId}:{objectId}:{action}:{occurred?.ToUniversalTime():O}",
            "notification" => $"{sourceId}:notification:{objectId}:{title}:{content}:{occurred?.ToUniversalTime():O}",
            _ => $"{sourceId}:event:{eventType}:{objectId}:{title}:{occurred?.ToUniversalTime():O}",
        };
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)));
    }

    private static string NormalizeEventType(string? header, string? payload)
    {
        string value = (header ?? payload ?? "event").Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_');
        return value switch
        {
            "pipeline_hook" or "pipeline" => "pipeline",
            "push_hook" or "push" => "push",
            "tag_push" => "tag_push",
            "merge_request" or "merge_request_hook" => "merge_request",
            "notification" => "notification",
            _ => "event",
        };
    }

    private static string? FirstString(JsonElement value, params string[] paths)
    {
        foreach (string path in paths)
        {
            JsonElement current = value;
            bool found = true;
            foreach (string part in path.Split('.'))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out current)) { found = false; break; }
            }
            if (!found || current.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) continue;
            if (current.ValueKind == JsonValueKind.String) return current.GetString();
            if (current.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False) return current.ToString();
        }
        return null;
    }

    private static long FirstLong(JsonElement value, params string[] paths) => long.TryParse(FirstString(value, paths), out long result) ? result : 0;
    private static double? FirstDouble(JsonElement value, params string[] paths) => double.TryParse(FirstString(value, paths), out double result) ? result : null;
    private static DateTimeOffset? FirstDate(JsonElement value, params string[] paths) => DateTimeOffset.TryParse(FirstString(value, paths), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset result) ? result : null;
    private static string Limit(string value, int length) => value.Length <= length ? value : value[..length];
}
