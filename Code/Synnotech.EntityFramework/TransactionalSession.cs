using System;
using System.Data;
using System.Data.Entity;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.EntityFramework;

/// <summary>
/// <para>
/// Represents an asynchronous session via an Entity Framework <see cref="DbContext" />. This session
/// is able to start and commit several transactions individually by calling <see cref="BeginTransaction" />.
/// </para>
/// <para>
/// Beware: you must not derive from this class and introduce other references to disposable objects.
/// Only the Context will be disposed.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">Your subtype that derives from <see cref="DbContext" />.</typeparam>
public abstract class TransactionalSession<TDbContext> : Session<TDbContext>, ITransactionalSession
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="AsyncTransactionalSession{TDbContext}" />.
    /// </summary>
    /// <param name="context">The EF DbContext used for database access.</param>
    /// <param name="disableQueryTracking"> The value indicating whether the query tracking behavior will be set to false (optional) </param>
    /// <param name="isolationLevel">
    /// The isolation level of the transaction (optional). If null is specified, no
    /// transaction will be started.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is null.</exception>
    protected TransactionalSession(TDbContext context, bool disableQueryTracking = false, IsolationLevel? isolationLevel = null) : base(context, disableQueryTracking, isolationLevel) { }

    /// <summary>
    /// <para>
    /// Begins a new transaction asynchronously. You must dispose the returned transaction by yourself. The
    /// session will not track any of the transactions that are created via this method.
    /// </para>
    /// <para>
    /// Be aware that commiting the transaction will not automatically call SaveChanges! You must call SaveChanges
    /// by yourself, otherwise committing will have no impact (this is how EF is implemented internally).
    /// </para>
    /// <para>
    /// Furthermore, you should ensure that a previous transaction has been committed before
    /// calling this method again - Synnotech.DatabaseAbstractions does not allow nested transactions.
    /// </para>
    /// </summary>
    public ITransaction BeginTransaction()
    {
        var transaction = Context.Database.BeginTransaction();
        return new EfTransaction(transaction);
    }
}