using System.Diagnostics.CodeAnalysis;

namespace AuditMod;

internal static class QueryExtensions
{
    public static Task<T?> FirstOrDefaultAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        this IQueryable<T> source,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) where T : class, new()
        => source.Where(predicate).FirstOrDefaultAsync(cancellationToken);

    public static Task<bool> AnyAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        this IQueryable<T> source,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) where T : class, new()
        => source.Where(predicate).AnyAsync(cancellationToken);

    public static Task<T?> SingleOrDefaultAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        this IQueryable<T> source,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) where T : class, new()
        => source.Where(predicate).SingleOrDefaultAsync(cancellationToken);
}
