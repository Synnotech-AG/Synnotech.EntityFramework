using System;
using System.Data;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.EntityFramework;

/// <summary>
/// <para>
/// Represents an asynchronous session via an Entity Framework <see cref="DbContext" />. This
/// type of session can be used to read as well as insert, update, or delete data. The session automatically
/// uses a transaction when calling <see cref="SaveChangesAsync" /> (as implemented in EF). If you
/// do not call <see cref="SaveChangesAsync" />, your changes will be discarded when the session is disposed.
/// </para>
/// <para>
/// Beware: you must not derive from this class and introduce other references to disposable objects.
/// Only the Context will be disposed.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">Your subtype that derives from <see cref="DbContext" />.</typeparam>
public abstract class AsyncSession<TDbContext> : AsyncReadOnlySession<TDbContext>, IAsyncSession
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="AsyncSession{TDbContext}" />.
    /// </summary>
    /// <param name="context">The EF DbContext used for database access.</param>
    /// <param name="isolationLevel">
    /// The isolation level of the transaction (optional). If null is specified, no
    /// transaction will be started.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is null.</exception>
    protected AsyncSession(TDbContext context, IsolationLevel? isolationLevel = null) : base(context, false, isolationLevel) { }

    /// <summary>
    /// Calls SaveChangesAsync on the internal DB context of Entity Framework.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Context.SaveChangesAsync(cancellationToken);
}