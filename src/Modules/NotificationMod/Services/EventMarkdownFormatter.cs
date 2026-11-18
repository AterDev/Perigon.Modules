using System.Text;
using Entity.NotificationMod;

namespace NotificationMod.Services;

public sealed class EventMarkdownFormatter
{
    public NotificationMessage Format(NotificationEvent value)
    {
        string title = value.EventType switch
        {
            "pipeline" => $"流水线 #{value.ObjectId} · {Status(value.Status)}",
            "push" => $"推送 · {Escape(value.Ref)}",
            "tag_push" => $"标签 · {Escape(value.Ref)}",
            "merge_request" => $"合并请求 !{value.ObjectId} · {Status(value.Status)}",
            _ => Escape(value.Title),
        };
        string text = $"## {title}\n\n> 项目：**{Escape(value.ProjectName)}**\n> 触发人：{Escape(value.ActorName)}\n> 状态：{Status(value.Status)}\n> 时间：{value.OccurredAt:O}\n\n{Escape(value.Content)}";
        string link = SafeUrl(value.Url);
        if (!string.IsNullOrWhiteSpace(link)) text += $"\n\n[查看详情]({link})";
        while (Encoding.UTF8.GetByteCount(text) > 4096 && text.Length > 32) text = text[..Math.Max(32, text.Length - 128)] + "…";
        return new NotificationMessage(title, text, link);
    }

    private static string Status(string value) => string.IsNullOrWhiteSpace(value) ? "未提供" : value.Length <= 40 ? value : value[..40];
    private static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("`", "\\`", StringComparison.Ordinal).Replace("*", "\\*", StringComparison.Ordinal).Replace("[", "\\[", StringComparison.Ordinal).Replace("]", "\\]", StringComparison.Ordinal);
    private static string SafeUrl(string value) => Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && uri.Scheme is "http" or "https" && string.IsNullOrEmpty(uri.UserInfo) ? value : "";
}
