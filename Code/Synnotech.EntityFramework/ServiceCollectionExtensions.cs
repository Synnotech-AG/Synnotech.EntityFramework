using System;
using System.Data.Entity;
using Light.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Synnotech.Core.DependencyInjection;

namespace Synnotech.EntityFramework;

/// <summary>
/// Provides extension methods for registering Entity Framework with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers your DB context with the DI container.
    /// </summary>
    /// <typeparam name="TDbContext">Your subtype that derives from <see cref="DbContext" />.</typeparam>
    /// <param name="services">The collection that holds all registrations for the DI container.</param>
    /// <param name="dbContextLifetime">
    /// The lifetime of the DB context (optional). The default value is <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is null.</exception>
    public static IServiceCollection AddDbContext<TDbContext>(this IServiceCollection services,
                                                              ServiceLifetime dbContextLifetime = ServiceLifetime.Transient)
        where TDbContext : DbContext
    {
        services.MustNotBeNull(nameof(services))
                .Add(new ServiceDescriptor(typeof(TDbContext), typeof(TDbContext), dbContextLifetime));
        return services;
    }

    /// <summary>
    /// Registers your Entity Framework settings with the DI container.
    /// </summary>
    /// <param name="services">The collection that holds all registrations for the DI container.</param>
    /// <param name="configurationSectionName">The configurationSectionName is used to get the correct section from your settings file. The default value is "database".</param>
    public static IServiceCollection AddEfSettings(this IServiceCollection services,
                                                   string configurationSectionName = EfSettings.DefaultSectionName) =>
        services.AddSingleton(container => EfSettings.FromConfiguration(container.GetRequiredService<IConfiguration>(), configurationSectionName));

    /// <summary>
    /// Registers your subclass of <see cref="EfSettings" /> with the DI container as a singleton.
    /// </summary>
    /// <typeparam name="T">The subclass of <see cref="EfSettings" />.</typeparam>
    /// <param name="services">The collection that holds all registrations for the DI container.</param>
    /// <param name="configurationSectionName">The configurationSectionName is used to get the correct section from your settings file. The default value is "database".</param>
    public static IServiceCollection AddEfSettings<T>(this IServiceCollection services,
                                                      string configurationSectionName = EfSettings.DefaultSectionName)
        where T : EfSettings =>
        services.AddSingleton(container => EfSettings.FromConfiguration<T>(container.GetRequiredService<IConfiguration>(), configurationSectionName));

    /// <summary>
    /// Enables logging in case you set IsLoggingEnabled in your EfSettings to true.
    /// </summary>
    /// <param name="context">The implementation of DbContext.</param>
    /// <param name="logger">The logger you are using.</param>
    /// <param name="efSettings">The settings instance. To enable logging, you must set IsLoggingEnabled to true.</param>
    /// <typeparam name="TDbContext">Your subtype that derives from <see cref="DbContext" />.</typeparam>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static TDbContext EnableLoggingIfNecessary<TDbContext>(this TDbContext context, ILogger logger, EfSettings efSettings)
        where TDbContext : DbContext
    {
        context.MustNotBeNull();
        efSettings.MustNotBeNull();
        logger.MustNotBeNull();
        if (efSettings.IsLoggingEnabled)
            context.Database.Log = message => logger.LogInformation(message);

        return context;
    }


    /// <summary>
    /// Registers a session with the DI container and a Func&lt;TAbstract&gt; factory delegate that can be used to
    /// resolve the session.
    /// </summary>
    /// <typeparam name="TAbstraction">The session abstraction.</typeparam>
    /// <typeparam name="TImplementation">The session implementation.</typeparam>
    /// <param name="services">The collection that holds all registrations for the DI container.</param>
    /// <param name="lifetime">
    /// The lifetime of the session (optional). The default value is <see cref="ServiceLifetime.Transient" />,
    /// i.e. callers must dispose the session by themselves.
    /// </param>
    /// <param name="registerFunc">
    /// The value indicating whether the factory delegate should be registered (optional). The default value
    /// is true. You can set this value to false if your DI container supports this automatically, e.g.
    /// LightInject supports Function Factories.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is null.</exception>
    public static IServiceCollection AddSession<TAbstraction, TImplementation>(this IServiceCollection services,
                                                                               ServiceLifetime lifetime = ServiceLifetime.Transient,
                                                                               bool? registerFunc = null)
        where TAbstraction : class, IDisposable
        where TImplementation : class, TAbstraction
    {
        services.MustNotBeNull();
        services.Add(new ServiceDescriptor(typeof(TAbstraction), typeof(TImplementation), lifetime));
        if (ContainerSettingsContext.Settings.CheckIfFactoryDelegateShouldBeRegistered(registerFunc))
            services.AddSingleton<Func<TAbstraction>>(container => container.GetRequiredService<TAbstraction>);
        return services;
    }
}