using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Light.GuardClauses;
using Microsoft.Extensions.Configuration;
using Synnotech.MsSqlServer;
using Synnotech.Xunit;
using Xunit;
using Xunit.Sdk;

namespace Synnotech.EntityFramework.Tests;

[TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
public sealed class ReadOnlySessionTests
{
    [Fact]
    public static void MustImplementIDisposable() =>
        typeof(ReadOnlySession<>).Should().Implement<IDisposable>();

    [SkippableFact]
    [TestOrder(1)]
    public static async Task CreateDatabaseTableAndInsertData()
    {
        Skip.IfNot(TestSettings.Configuration.GetValue<bool>("integrationTests:areTestsEnabled"));

        var targetConnectionString = TestSettings.Configuration["integrationTests:connectionString"];

        if (targetConnectionString.IsNullOrWhiteSpace())
            throw new XunitException("You must configure migrationDatabase in testsettings.json when setting executeMigrationTests to true.");


        await Database.DropAndCreateDatabaseAsync(targetConnectionString);
        using var sqlConnection = await Database.OpenConnectionAsync(targetConnectionString);
        await sqlConnection.CreateTemporaryTableAsync();
        await sqlConnection.InsertPersonsAsync();
    }


    [SkippableFact]
    [TestOrder(2)]
    public static void ReadData()
    {
        Skip.IfNot(TestSettings.Configuration.GetValue<bool>("integrationTests:areTestsEnabled"));
        var persons = new Person[]
        {
            new () { Id = 1, Name = "Doe", FirstName = "John", Age = 43 },
            new () { Id = 2, Name = "Dos Santos", FirstName = "Maria", Age = 24 },
        };
        var targetConnectionString = TestSettings.Configuration["integrationTests:connectionString"];

        var context = new TestContext(targetConnectionString);

        List<Person> personsFromDb;
        using (var session = new EfGetPersonsSession(context))
        {
            personsFromDb = session.GetPersons();
        }

        personsFromDb.Should().BeEquivalentTo(persons);
    }

    private interface IGetPersonSession : IDisposable
    {
        List<Person> GetPersons();
    }

    private sealed class EfGetPersonsSession : ReadOnlySession<TestContext>, IGetPersonSession
    {
        public EfGetPersonsSession(TestContext context) : base(context) { }

        public List<Person> GetPersons() => Context.Persons.ToList();
    }
}