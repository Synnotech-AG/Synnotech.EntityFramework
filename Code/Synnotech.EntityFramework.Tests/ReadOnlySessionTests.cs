using System;
using FluentAssertions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public static class ReadOnlySessionTests
{
    [Fact]
    public static void MustImplementIDisposable() =>
        typeof(ReadOnlySession<>).Should().Implement<IDisposable>();
}