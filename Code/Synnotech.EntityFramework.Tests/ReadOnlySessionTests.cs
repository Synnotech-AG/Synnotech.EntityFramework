using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Synnotech.Xunit;
using Xunit;

namespace Synnotech.EntityFramework.Tests;

public class ReadOnlySessionTests
{
    [SkippableFact]
    public static async Task ReadData()
    {
        Skip.IfNot(TestSettings.Configuration.GetValue<bool>("integrationTests:areTestsEnabled"));

        throw new NotImplementedException("");
    }
}