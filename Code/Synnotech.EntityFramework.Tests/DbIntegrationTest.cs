using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Synnotech.MsSqlServer;
using Synnotech.Xunit;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public abstract class DbIntegrationTest : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        if (!CheckIfIntegrationTestsAreEnabled())
            return;

        var connectionString = GetConnectionString();

        await Database.DropAndCreateDatabaseAsync(connectionString);
        using var connection = await Database.OpenConnectionAsync(connectionString);
        await connection.CreatePersonsTableAsync();
        await connection.InsertPersonsAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static bool CheckIfIntegrationTestsAreEnabled() =>
        TestSettings.Configuration.GetValue<bool>("integrationTests:areTestsEnabled");

    private static string GetConnectionString() =>
        TestSettings.Configuration["integrationTests:connectionString"];

    protected static string GetConnectionStringOrSkip()
    {
        Skip.IfNot(CheckIfIntegrationTestsAreEnabled());
        return GetConnectionString();
    }
}