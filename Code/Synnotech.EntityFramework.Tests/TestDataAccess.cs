using System.Data.Entity;
using FluentAssertions;

namespace Synnotech.EntityFramework.Tests;

public class TestContext : DbContext
{
    public TestContext(string connectionString) : base(connectionString) { }

    public DbSet<Person> Persons => Set<Person>();

}

public class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public int Age { get; set; }
}