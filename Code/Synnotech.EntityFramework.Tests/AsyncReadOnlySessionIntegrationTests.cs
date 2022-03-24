using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public sealed class AsyncReadOnlySessionIntegrationTests : DbIntegrationTest
{
    [SkippableFact]
    public async Task ReadDataAsync()
    {
        var connectionString = GetConnectionStringOrSkip();

        var context = new TestContext(connectionString);
        var expectedContacts = new Contact[]
        {
            new () { Id = 1, LastName = "Doe", FirstName = "John", Age = 43 },
            new () { Id = 2, LastName = "Dos Santos", FirstName = "Maria", Age = 24 },
        };

        await using var session = new EfGetContactAsyncSession(context);
        var contacts = await session.GetContactsAsync();

        contacts.Should().BeEquivalentTo(expectedContacts);
    }

    private interface IGetContactSessionAsync
    {
        Task<List<Contact>> GetContactsAsync();
    }

    private sealed class EfGetContactAsyncSession : AsyncReadOnlySession<TestContext>, IGetContactSessionAsync
    {
        public EfGetContactAsyncSession(TestContext context) : base(context) { }

        public Task<List<Contact>> GetContactsAsync() =>
            Context.Contacts.ToListAsync();
    }
}