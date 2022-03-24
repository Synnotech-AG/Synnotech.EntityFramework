# Synnotech.EntityFramework
Implements Synnotech.DatabaseAbstractions for Entity Framework

[![Synnotech Logo](synnotech-large-logo.png)](https://www.synnotech.de/)


[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Synnotech-AG/Synnotech.EntityFramework/blob/main/LICENSE)

# How to install
The Synnotech.EntityFramework packages are compiled against [.NET Standard 2.1 and .NET Framework 47](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) and thus supports all major plattforms like .NET 6, .NET Core, .NET Framework 4.7 or newer, Mono, Xamarin, UWP, or Unity.

# Writing custom sessions

When writing code that performs I/O, we usually write custom abstractions, containing a single method for each I/O request. The following sections show you how to design abstractions, implement them, and call them in client code.

## Sessions that only read data

The following code snippets show the example for an ASP.NET Core controller that represents an HTTP GET operation for contacts.

Your I/O abstraction should simply derive from `IAsyncReadOnlySession` and offer the corresponding I/O call to load contacts:

```csharp
public interface IGetContactsSession : IAsyncReadOnlySession
{
    Task<List<Contact>> GetContactsAsync(int skip, int take);
}
```

To implement this interface, you should derive from the `AsyncReadOnlySession<T>` class of Synnotech.EntityFramework:

```csharp
// DatabaseContext is your custom class deriving from EF's DbContext
public sealed class EfGetContactsSession : AsyncReadOnlySession<DatabaseContext>, IGetContactsSession
{
    public EfGetContactsSession(DatabaseContext context) : base(context) { }

    public Task<List<Contact>> GetContactsAsync(int skip, int take) =>
        Context.Contacts
               .OrderBy(contact => contact.LastName)
               .Skip(skip)
               .Take(take)
               .ToListAsync();
}
```

`AsyncReadOnlySession<T>` implements `IAsyncReadOnlySession`, `IDisposable` and `IAsyncDisposable` for you and provides EF's `DbContext` via a protected property (the one that is passed in via constructor injection). This reduces the code you need to write in your session for your specific use case.

You can then consume your session via the abstraction in client code. Check out the following ASP.NET Core controller for example:

```csharp
[ApiController]
[Route("api/contacts")]
public sealed class GetContactsController : ControllerBase
{
    public GetContactsController(Func<IGetContactsSession> createSession) =>
        CreateSession = createSession;
        
    private Func<IGetContactsSession> CreateSession { get; }
    
    [HttpGet]
    public async Task<ActionResult<List<ContactDto>>> GetContacts(int skip, int take)
    {
        if (this.CheckPagingParametersForErrors(skip, take, out var badResult))
            return badResult;
        
        await using var session = CreateSession();
        var contacts = await session.GetContactsAsync(skip, take);
        return ContactDto.FromContacts(contacts); // Or use an object-to-object mapper
    }
}
```

In this example, an `Func<IGetContactsSession>` is injected into the controller. This factory delegate is used to instantiate the session once the parameters are validated. After that, the contacts are retrieved via `await session.GetContactsAsync(skip, take)`, transformed to DTOs and returned from the controller.

```csharp
// This call will perform the following registrations (with the default settings):
// services.AddTransient<IGetContactsSession, EfGetContactsSession>()>();
// services.AddSingleton<Func<IGetContactsSession>>(container => container.GetRequiredService<IGetContactsSession>);
services.AddSession<IGetContactsSession, EfGetContactsSession>();
```

Please note:

- when you derive from `AsyncReadOnlySession<T>`, change tracking is disabled by default. This is because read-only sessions only read data and return them. You can change this behavior by setting the second constructor parameter `disableQueryTracking` to false.
- `AddSession` registers the factory delegate `Func<IGetContactsSession` with the DI container by default. We recommend to use a proper DI container like [LightInject](https://github.com/seesharper/LightInject) instead of Microsoft.Extensions.DependencyInjection. LightInject offers [Function Factories](https://www.lightinject.net/#function-factories) for free, so you can set `registerFunc` to false when calling `AddSession`.

## Sessions that use a single transaction

If you want to insert, update or delete data, then you usually want to use a single transaction for your database commands. You can use the `IAsyncSession` interface for these scenarios and implement your custom session by deriving from `AsyncSession<T>`.

The abstraction might look like this:

```csharp
public interface IUpdateContactSession : IAsyncSession
{
    Task<Contact?> GetContactAsync(int id);
}
```

The class that implements this interface should derive from `AsyncSession<T>` which provides the same members as `AsyncReadOnlySession<T>` plus a `SaveChangesAsync` method:

```csharp
// DatabaseContext is your custom class deriving from EF's DbContext
public sealed class EfUpdateContactSession : AsyncSession<DatabaseContext>, IUpdateContactSession
{
    public EfUpdateContactSession(DatabaseContext context) : base(context) { }

    public Task<Contact?> GetContactAsync(int id) =>
        Context.Set<Contact?>().FindAsync(id);
}
```

You should register your session with the DI container, the same way as we did it for the read-only session:

```csharp
services.AddSession<IUpdateContactSession, EfUpdateContactSession>();
```

Your controller could then use the factory to open the session asynchronously:

```csharp
[ApiController]
[Route("api/contacts/update")]
public sealed class UpdateContactController : ControllerBase
{
    public UpdateContactController(Func<IUpdateContactSession> createSession,
                                   ContactValidator validator)
    {
        CreateSession = createSession;
        Validator = validator;
    }
    
    private Func<IUpdateContactSession> CreateSession { get; }
    private ContactValidator Validator { get; }
    
    [HttpPut]
    public async Task<IActionResult> UpdateContact(ContactDto contactDto)
    {
        if (this.CheckForErrors(contactDto, Validator, out var badResult))
            return badResult;
            
        await using var session = CreateSession();
        var contact = await session.GetContactAsync(contactDto.Id);
        if (contact == null)
            return NotFound();
        contactDto.UpdateContact(contact); // Or use an object-to-object mapper
        await session.SaveChangesAsync(); // Changes are saved via EF's change tracking mechanism
        return NoContent();
    }
}
```

*Please note:* Synnotech.EntityFramework also supports scenarios when the session is registered with a scoped lifetime (the session is then only initialized once per request and disposed by the DI container at the end of the request). However, we recommend that you use a transient lifetime as we argue that it is the controller's responsibility to begin and end the database session. This way, you can more easily test the whole controller without spinning up the ASP.NET Core runtime in your tests.

## Sessions that use multiple transactions

If you need to handle transactions individually, (e.g. because you want to handle a large amount of data in batches and have a transaction per batch), we recommend that you create a session per batch:

```csharp
public interface IUpdateProductsSession : IAsyncSession
{
    Task<int> GetProductCountAsync();

    Task<List<Product>> GetProductBatchAsync(int skip, int take);
}
```

The implementation of this session could look like this:

```csharp
public sealed class EfUpdateProductsSession : AsyncSession<DatabaseContext>, IUpdateProductsSession
{
    public EfUpdateProductsSession(DatabaseContext context) : base(context) { }

    public Task<int> GetProductsCountAsync() => Context.Products.CountAsync();

    public Task<List<Product>> GetProductBatchAsync(int skip, int take) =>
        Context.Products
               .OrderBy(product => product.Id)
               .Skip(skip)
               .Take(take)
               .ToListAsync();
}
```

Your job that updates all products might look like this:

```csharp
public sealed class UpdateAllProductsJob
{
    public UpdateAllProductsJob(Func<IUpdateProductsSession> createSession, ILogger logger)
    {
        CreateSession = createSession;
        Logger = logger;
    }
    
    private Func<IUpdateProductsSession> CreateSession { get; }
    private ILogger Logger { get; }

    public async Task UpdateProductsAsync()
    {
        var session = CreateSession();
        var numberOfProducts = await session.GetProductsCountAsync();
        const int batchSize = 100;
        var skip = 0;
        while (skip < numberOfProducts)
        {
            try
            {
                var products = await session.GetProductBatchAsync(skip, batchSize);
                foreach (var product in products)
                {
                    product.TryPerformDailyUpdate(Logger);
                }

                await session.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Batch {From} to {To} could not be updated properly", skip + 1, batchSize + skip);
            }
            finally
            {
                await session.DisposeAsync();
            }

            skip += batchSize;
            session = CreateSession();
        }
    }
}
```

In the example above, the job get an `Func<IUpdateProductsSession>` that can be used to create a session. In `UpdateProductsAsync`, the session is created and the number of products is determined. The products are then updated in batches with size 100. After each batch, a new session (and therefor) is started and committed at the end. The transaction is disposed in the finally block before a new batch begins.

For this to work, you must register the session with the DI container:

```csharp
services.AddSession<IUpdateProductsSession, EfUpdateProductsSession>();
```

*Please note*: while there is an implementation of `IAsyncTransactionalSession` in this package, we do not recommend using it. The pattern above is easier to maintain and better follows the recommendations of Entity Framework.

# Non-Tracked Set

By default, EF enables change tracking when querying data. This means that for every entity in the result set, a copy will be created that is used to determine which changes need to be made (especially for updates).

This change tracking mechanism comes with an overhead. If you know in advance that you will not update the resulting graph, then you can disable change tracking by using the `NonTrackedSet` extension method.

```csharp
public sealed class MySession : AsyncSession<DatabaseContext>, IMySession
{
    public MySession(DatabaseContext context) : base(context) { }

    public Task<List<Contact>> LoadContactsAsync() =>
        Context.NonTrackedSet<Contact>() // This will disable change tracking
               .ToListAsync();
}
```

# General recommendations

1. All I/O should be abstracted. You should create abstractions that are specific for your use cases.
2. Your custom abstractions should derive from `IAsyncReadOnlySession` (or `IReadOnlySession` for synchronous sessions) (when they only read data) or from `IAsyncSession` (or `ISession` for synchronous sessions) (when they also manipulate data and therefore need a transaction and change tracking).
3. Prefer async I/O over sync I/O. Threads that wait for a database query to complete can handle other requests in the meantime when the query is performed asynchronously. This prevents thread starvation under high load and allows your web service to scale better.
4. In case of web apps, we do not recommend using the DI container to dispose of the session. Instead, it is the controller's responsibility to do that. This way you can easily test the controller without running the whole ASP.NET Core infrastructure in your tests. To make your life easier, use an appropriate DI container like [LightInject](https://github.com/seesharper/LightInject) instead of Microsoft.Extensions.DependencyInjection. These more sophisticated DI containers provide you with more features, e.g. [Function Factories](https://www.lightinject.net/#function-factories).