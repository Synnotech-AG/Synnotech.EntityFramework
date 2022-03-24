using FluentAssertions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public static class TransactionalTests
{
    [Fact]
    public static void MustImplementITransactionalSession() =>
        typeof(TransactionalSession<>).Should().Implement<ITransactionalSession>();

    [Fact]
    public static void MustDeriveFromSession() =>
        typeof(TransactionalSession<>).Should().BeDerivedFrom(typeof(Session<>));
}