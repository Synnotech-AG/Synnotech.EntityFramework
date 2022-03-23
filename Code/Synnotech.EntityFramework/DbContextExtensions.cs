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
#if NETSTANDARD2_1
        /// <summary>
        /// Returns an IQueryable&lt;T&gt; of the specified type with change tracking disabled.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="context">The context that the query is performed upon.</param>
        /// <param name="withIdentityResolution">
        /// The value indicating whether EF's Identity Resolution algorithm should be enabled or not (optional).
        /// The default value is true. If this value is set to true, EF will only instantiate
        /// one entity and reference it in all other entities, instead of instantiating it multiple times (one per reference).
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public static IQueryable<T> NonTrackedSet<T>(this DbContext context, bool withIdentityResolution = true)
            where T : class
        {
            var query = context.MustNotBeNull(nameof(context))
                               .Set<T>();
            return withIdentityResolution ? query.AsNoTrackingWithIdentityResolution() : query.AsNoTracking();
        }
#else
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
#endif
}