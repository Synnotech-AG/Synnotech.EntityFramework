using System;
using System.Data.Entity;
using System.Linq;
using Light.GuardClauses;

namespace Synnotech.EntityFramework;

/// <summary>
/// Provides extension methods for <see cref="DbContext" />.
/// </summary>
public static class DbContextExtensions
{

    /// <summary>
    /// Returns an IQueryable&lt;T&gt; of the specified type with change tracking disabled.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context that the query is performed upon.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is null.</exception>
    public static IQueryable<T> NonTrackedSet<T>(this DbContext context)
        where T : class =>
        context.MustNotBeNull(nameof(context))
               .Set<T>()
               .AsNoTracking();
}