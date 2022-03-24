using System.Threading.Tasks;
using FluentAssertions;
using Synnotech.DatabaseAbstractions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public sealed class AsyncSessionIntegrationTests : DbIntegrationTest
{
    [SkippableFact]
    public async Task InsertNewContactAsync()
    {
        var connectionString = GetConnectionStringOrSkip();
        var context = new TestContext(connectionString);
        var contact = new Contact { LastName = "Mustermann", FirstName = "Max", Age = 27 };

        await using (var session = new EfAddContactSession(context))
        {
            session.AddContact(contact);
            await session.SaveChangesAsync();
        }

        contact.Id.Should().Be(3);
        context.SaveChangesMustHaveBeenCalled();
    }


    private interface IAddContactSessionAsync : IAsyncSession
    {
        void AddContact(Contact contact);
    }

    private sealed class EfAddContactSession : AsyncSession<TestContext>, IAddContactSessionAsync
    {
        public EfAddContactSession(TestContext context) : base(context) { }

        public void AddContact(Contact contact) =>
            Context.Contacts.Add(contact);
    }
}