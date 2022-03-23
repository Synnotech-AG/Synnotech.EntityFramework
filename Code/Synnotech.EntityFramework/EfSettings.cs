using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Synnotech.EntityFramework;

/// <summary>
/// Represents the default settings for Entity Framework.
/// </summary>
public class EfSettings
{
    /// <summary>
    /// The default section name within the <see cref="IConfiguration" /> where settings are loaded from.
    /// </summary>
    public const string DefaultSectionName = "database";

    /// <summary>
    /// Gets or sets the connection string to the target database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

     /// <summary>
    /// Gets or sets the value indicating whether Entity Framework statement logging is enabled. The default value is false.
    /// </summary>
    public bool IsLoggingEnabled { get; set; }

    /// <summary>
    /// Loads the <see cref="EfSettings" /> from configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance where the settings are loaded from.</param>
    /// <param name="sectionName">The name of the section that represents the EF settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration" /> or <paramref name="sectionName" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sectionName" /> is an empty string or contains only whitespace.</exception>
    /// <exception cref="InvalidConfigurationException">Thrown when the settings could not be loaded (most likely because the section is not present in the configuration).</exception>
    public static EfSettings FromConfiguration(IConfiguration configuration, string sectionName = DefaultSectionName) =>
        FromConfiguration<EfSettings>(configuration, sectionName);

    /// <summary>
    /// Loads the EF settings from configuration.
    /// </summary>
    /// <typeparam name="T">The type of EF settings that will be used to load the settings.</typeparam>
    /// <param name="configuration">The configuration instance where the settings are loaded from.</param>
    /// <param name="sectionName">The name of the section that represents the EF settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration" /> or <paramref name="sectionName" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sectionName" /> is an empty string or contains only whitespace.</exception>
    /// <exception cref="InvalidConfigurationException">Thrown when the settings could not be loaded (most likely because the section is not present in the configuration).</exception>
    public static T FromConfiguration<T>(IConfiguration configuration, string sectionName = DefaultSectionName)
    {
        configuration.MustNotBeNull(nameof(configuration));
        sectionName.MustNotBeNullOrWhiteSpace(nameof(sectionName));
        return configuration.GetSection(sectionName)
                            .Get<T?>() ?? throw new InvalidConfigurationException($"EF settings could not be retrieved from configuration section \"{sectionName}\".");
    }
}