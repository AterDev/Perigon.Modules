using System.Linq.Expressions;

namespace ResourceMod;

/// <summary>
/// Compatibility overloads for the predicate-shaped LINQ calls used by the
/// module. Perigon.PostgreSQL intentionally keeps predicates as Where clauses.
/// </summary>
internal static class QueryExtensions
{
    public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> source,
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        where T : class, new()
        => source.Where(predicate).FirstOrDefaultAsync(cancellationToken);

    public static Task<bool> AnyAsync<T>(this IQueryable<T> source,
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        where T : class
        => source.Where(predicate).AnyAsync(cancellationToken);
}
