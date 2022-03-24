using System.Data.Entity;

namespace Synnotech.EntityFramework.Tests;

public class TestContext : DbContext
{
    static TestContext() =>
        Database.SetInitializer<TestContext>(null);

    public TestContext(string connectionString) : base(connectionString) { }

#nullable disable
    public DbSet<Contact> Contacts { get; set; }
#nullable restore
}

public class Contact
{
    public int Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public int Age { get; set; }
}