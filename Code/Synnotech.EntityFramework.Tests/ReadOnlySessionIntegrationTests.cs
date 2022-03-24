using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public sealed class ReadOnlySessionIntegrationTests : DbIntegrationTest
{
    [SkippableFact]
    public void ReadData()
    {
        var connectionString = GetConnectionStringOrSkip();

        var context = new TestContext(connectionString);
        List<Contact> contacts;
        using (var session = new EfGetPersonsSession(context))
        {
            contacts = session.GetContacts();
        }

        var expectedPersons = new Contact[]
        {
            new () { Id = 1, LastName = "Doe", FirstName = "John", Age = 43 },
            new () { Id = 2, LastName = "Dos Santos", FirstName = "Maria", Age = 24 },
        };
        contacts.Should().BeEquivalentTo(expectedPersons);
        context.ShouldBeDisposed();
    }

    private interface IGetPersonSession : IDisposable
    {
        List<Contact> GetContacts();
    }

    private sealed class EfGetPersonsSession : ReadOnlySession<TestContext>, IGetPersonSession
    {
        public EfGetPersonsSession(TestContext context) : base(context) { }

        public List<Contact> GetContacts() => Context.Contacts.ToList();
    }
}