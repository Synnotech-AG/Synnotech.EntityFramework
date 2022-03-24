using FluentAssertions;
using Synnotech.DatabaseAbstractions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public static class AsyncReadOnlySessionTests
{
    [Fact]
    public static void MustImplementIAsyncReadOnlySession() =>
        typeof(AsyncReadOnlySession<>).Should().Implement<IAsyncReadOnlySession>();

}