using FluentAssertions;
using Synnotech.DatabaseAbstractions;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public sealed class SessionIntegrationTests : DbIntegrationTest
{
    [SkippableFact]
    public void InsertNewContact()
    {
        var connectionString = GetConnectionStringOrSkip();
        var context = new TestContext(connectionString);
        var contact = new Contact { LastName = "Mustermann", FirstName = "Max", Age = 27 };


        using (var session = new EfAddContactSession(context))
        {
            session.AddContact(contact);
            session.SaveChanges();
        }

        contact.Id.Should().Be(3);
        context.SaveChangesMustHaveBeenCalled();
        context.ShouldBeDisposed();
    }

    private interface IAddContactSession : ISession
    {
        void AddContact(Contact contact);
    }

    private sealed class EfAddContactSession : Session<TestContext>, IAddContactSession
    {
        public EfAddContactSession(TestContext context) : base(context) { }

        public void AddContact(Contact contact) =>
            Context.Contacts.Add(contact);
    }
}