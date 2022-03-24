using FluentAssertions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public static class AsyncTransactionalTests
{
    [Fact]
    public static void MustImplementIAsyncTransactionalSession() =>
        typeof(AsyncTransactionalSession<>).Should().Implement<IAsyncTransactionalSession>();

    [Fact]
    public static void MustDeriveFromAsyncSession() =>
        typeof(AsyncTransactionalSession<>).Should().BeDerivedFrom(typeof(AsyncSession<>));
}