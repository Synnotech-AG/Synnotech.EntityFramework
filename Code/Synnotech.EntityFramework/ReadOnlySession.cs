using System;
using System.Data;
using System.Data.Entity;
using Light.GuardClauses;

namespace Synnotech.EntityFramework;

/// <summary>
/// <para>
/// Represents a synchronous database session via an Entity Framework's <see cref="DbContext" />. This
/// session is only used to read data (i.e. no data is inserted, updated, or deleted), thus SaveChanges
/// is not available. No transaction is needed while this session is active.
/// </para>
/// <para>
/// Beware: you must not derive from this class and introduce other references to disposable objects.
/// Only the <see cref="Context" /> and the optional transaction will be disposed.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">Your subtype that derives from <see cref="DbContext" />.</typeparam>
public abstract class ReadOnlySession<TDbContext> : IDisposable
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="ReadOnlySession{TDbContext}" />.
    /// </summary>
    /// <param name="context">The EF DbContext used for database access.</param>
    /// <param name="disableQueryTracking"> The value indicating whether the query tracking behavior will be disabled (optional).</param>
    /// <param name="isolationLevel">
    /// The isolation level of the transaction (optional). If null is specified, no
    /// transaction will be started.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is null.</exception>
    protected ReadOnlySession(TDbContext context, bool disableQueryTracking = false, IsolationLevel? isolationLevel = null)
    {
        Context = context.MustNotBeNull();
        if (disableQueryTracking)
            context.Configuration.AutoDetectChangesEnabled = false;
        if (isolationLevel.HasValue)
            Transaction = Context.Database.BeginTransaction(isolationLevel.Value);
    }

    /// <summary>
    /// Gets the DB context of Entity Framework.
    /// </summary>
    protected TDbContext Context { get; }

    /// <summary>
    /// Gets the transaction that is associated with this session. This property might be null.
    /// </summary>
    protected DbContextTransaction? Transaction { get; }

    /// <summary>
    /// Disposes the DB context and the optional transaction.
    /// </summary>
    public void Dispose()
    {
        Transaction?.Dispose();
        Context.Dispose();
    }
}