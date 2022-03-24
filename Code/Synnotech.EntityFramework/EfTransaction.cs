using System;
using System.Data.Entity;
using Light.GuardClauses;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.EntityFramework;

/// <summary>
/// Represents an adapter for the Entity Framework <see cref="DbContextTransaction" /> that
/// implements <see cref="ITransaction" />. The transaction will be implicitly rolled back
/// when commit was not called and the transaction was disposed.
/// </summary>
public sealed class EfTransaction : ITransaction
{
    /// <summary>
    /// Initializes a new instance of <see cref="EfTransaction" />.
    /// </summary>
    /// <param name="transaction">The EF transaction that will be wrapped by this instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transaction" /> is null.</exception>
    public EfTransaction(DbContextTransaction transaction) =>
        Transaction = transaction.MustNotBeNull(nameof(transaction));


    private DbContextTransaction Transaction { get; }

    /// <summary>
    /// Disposes the underlying transaction. It will also be rolled back if <see cref="Commit" />
    /// was not called.
    /// </summary>
    public void Dispose() => Transaction.Dispose();

    /// <summary>
    /// Commits the underlying EF transaction.
    /// </summary>
    public void Commit() => Transaction.Commit();
}