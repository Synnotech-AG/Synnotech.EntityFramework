using System;
using System.Data;
using System.Data.Entity;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.EntityFramework;

/// <summary>
/// <para>
/// Represents a session via an Entity Framework  <see cref="DbContext" />. This
/// type of session can be used to read as well as insert, update, or delete data. The session automatically
/// uses a transaction when calling <see cref="SaveChanges" /> (as implemented in EF). If you
/// do not call <see cref="SaveChanges" />, your changes will be discarded when the session is disposed.
/// </para>
/// <para>
/// Beware: you must not derive from this class and introduce other references to disposable objects.
/// Only the Context will be disposed.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">Your subtype that derives from <see cref="DbContext" />.</typeparam>
public abstract class Session<TDbContext> : ReadOnlySession<TDbContext>, ISession
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="AsyncSession{TDbContext}" />.
    /// </summary>
    /// <param name="context">The EF DbContext used for database access.</param>
    /// <param name="disableQueryTracking"> The value indicating whether the query tracking behavior will be set to false (optional) </param>
    /// <param name="isolationLevel">
    /// The isolation level of the transaction (optional). If null is specified, no
    /// transaction will be started.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is null.</exception>
    protected Session(TDbContext context,bool disableQueryTracking = false, IsolationLevel? isolationLevel = null) : base(context, disableQueryTracking, isolationLevel) { }

    /// <summary>
    /// Calls SaveChanges on the internal DB context of Entity Framework Core.
    /// </summary>
    public void SaveChanges()
    {
        Context.SaveChanges();
    }
}