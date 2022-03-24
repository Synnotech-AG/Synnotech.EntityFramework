using FluentAssertions;
using Synnotech.DatabaseAbstractions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public static class SessionTests
{
    [Fact]
    public static void MustImplementISession() =>
        typeof(Session<>).Should().Implement<ISession>();

    [Fact]
    public static void MustDeriveFromReadOnlySession() => 
        (typeof(Session<>)).Should().BeDerivedFrom(typeof(ReadOnlySession<>));
}