using System.Data.Entity;
using FluentAssertions;

namespace Synnotech.EntityFramework.Tests;

public class TestContext : DbContext
{
    static TestContext() =>
        Database.SetInitializer<TestContext>(null);

    public TestContext(string connectionString) : base(connectionString) { }

    private int DisposeCount { get; set; }

#nullable disable
    public DbSet<Contact> Contacts { get; set; }
#nullable restore

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DisposeCount++;
    }

    public void ShouldBeDisposed() =>
        DisposeCount.Should().BeGreaterOrEqualTo(1);
}

public class Contact
{
    public int Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public int Age { get; set; }
}