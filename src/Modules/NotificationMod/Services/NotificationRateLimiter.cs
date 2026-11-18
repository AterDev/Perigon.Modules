using System.Collections.Concurrent;

namespace NotificationMod.Services;

public sealed class NotificationRateLimiter
{
    private readonly ConcurrentDictionary<Guid, Queue<DateTimeOffset>> _windows = new();

    public bool TryAcquire(Guid receiverId, int limit)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Queue<DateTimeOffset> window = _windows.GetOrAdd(receiverId, static _ => new Queue<DateTimeOffset>());
        lock (window)
        {
            while (window.Count > 0 && now - window.Peek() >= TimeSpan.FromMinutes(1)) window.Dequeue();
            if (window.Count >= limit) return false;
            window.Enqueue(now);
            return true;
        }
    }
}
