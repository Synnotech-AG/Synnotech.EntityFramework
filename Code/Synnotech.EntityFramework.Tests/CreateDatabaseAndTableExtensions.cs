using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Synnotech.EntityFramework.Tests;

public static class CreateDatabaseAndTableExtensions
{
    public static async Task CreatePersonsTableAsync(this SqlConnection sqlConnection)
    {
        using var command = sqlConnection.CreateCommand();
        command.CommandText = @"
CREATE TABLE Contacts (
    Id INT IDENTITY(1,1) Primary Key, 
    LastName NVARCHAR(100) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    Age INT NOT NULL
);";
        await command.ExecuteNonQueryAsync();
    }

    public static async Task InsertPersonsAsync(this SqlConnection sqlConnection)
    {
        using var command = sqlConnection.CreateCommand();
        command.CommandText = @"
INSERT INTO Contacts (LastName, FirstName, Age)
VALUES('Doe','John',43);";
        await command.ExecuteNonQueryAsync();
        command.CommandText = @"
INSERT INTO Contacts (LastName, FirstName, Age)
VALUES('Dos Santos','Maria',24);";
        await command.ExecuteNonQueryAsync();
    }
}