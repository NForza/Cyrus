
# Cyrus - An Opinionated Framework for Creating CQRS Applications and Web APIs

Cyrus is a CQRS framework that focuses on ease of use, simplicity, and minimizing boilerplate code. It incorporates principles from [Domain-Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design) to enable the rapid and pragmatic creation of Web APIs. Cyrus leverages [Roslyn Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md) to generate code rather than relying on reflection.

Cyrus generates code for the following components:

- [Strongly Typed IDs](#strongly-typed-ids)
- [Queries and Query Handlers](#queries-and-query-handlers)
- [Commands and Command Handlers](#commands-and-command-handlers)
- [Exposing Commands and Queries in a Web API](#exposing-commands-and-queries-in-a-web-api)
- [Application Startup](#application-startup)

### Strongly Typed IDs

Cyrus allows you to define strongly typed IDs by defining a `partial record struct` like this:

```csharp
[StringId(3, 200)]
public partial record struct Address;
```

This generates additional code that simplifies using this type in your code, including a custom `JsonConverter`, a `TypeConverter`, casting operators, and other methods.

```csharp
[JsonConverter(typeof(AddressJsonConverter))]
[TypeConverter(typeof(AddressTypeConverter))]
public partial record struct Address(string Value) : ITypedId
{
    public static Address Empty => new Address(string.Empty);
    
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
    
    public static implicit operator string(DemoApp.Contracts.Address typedId) => typedId.Value;
    public static explicit operator DemoApp.Contracts.Address(string value) => new(value);
    
    public bool IsValid() => !string.IsNullOrEmpty(Value) && Value.Length >= 3 && Value.Length <= 200;
    
    public override string ToString() => Value?.ToString();
}
```

For GUID-based strongly typed IDs, you can use the `GuidId` attribute:

```csharp
[GuidId]
public partial record struct CustomerId;
```

The generated code for a `Guid`-based ID looks like this:

```csharp
[JsonConverter(typeof(CustomerIdJsonConverter))]
[TypeConverter(typeof(CustomerIdTypeConverter))]
public partial record struct CustomerId(Guid Value) : ITypedId
{
    public CustomerId() : this(Guid.NewGuid()) { }
    public static CustomerId Empty => new CustomerId(Guid.Empty);
    
    public static implicit operator System.Guid(DemoApp.Contracts.CustomerId typedId) => typedId.Value;
    public static explicit operator DemoApp.Contracts.CustomerId(System.Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}
```

### Queries and Query Handlers

Query Handlers process queries to retrieve data. A Query object is defined as a simple `record` or `record struct`:

```csharp
public record struct CustomerByIdQuery(CustomerId Id);
```

A simplified handler for this query might look like this:

```csharp
public class CustomersQueryHandler
{
    public static Customer Query(CustomerByIdQuery query)
    {
        return new Customer(query.Id, new($"Customer-{query.Id}"));
    }
}
```

Query handlers can return any type of object, and can be either static or instance methods. They can also be synchronous or asynchronous. Here are valid variations of the query handler method:

```csharp
public Customer Query(CustomerByIdQuery query)
public static Customer Query(CustomerByIdQuery query)
public static async Task<Customer> Query(CustomerByIdQuery query)
public async Task<Customer> Query(CustomerByIdQuery query)
```

No extra configuration is needed to hook up queries and their handlers. The code generation connects queries to handlers based on these conventions:

- A Query class name must end with `Query`
- Handler methods must be named `Query`
- Handler methods must take one parameter, which should be a `Query` class

Dependency injection is supported for instance methods, both synchronous and asynchronous.

### Commands and Command Handlers

Commands and their handlers are structured similarly to queries. For example:

```csharp
public record struct AddCustomerCommand(Name Name, Address Address);

public class AddCustomerCommandHandler
{
    public CommandResult Execute(AddCustomerCommand command)
    {
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        return new CommandResult(new CustomerAddedEvent(new CustomerId(), command.Name, command.Address));
    }
}
```

Command handlers must return either `CommandResult` or `Task<CommandResult>`. Events are dispatched through the system via an event bus, and you can use a local bus or integrate with an external event bus using [MassTransit](https://masstransit.io/).

The conventions for command handlers are as follows:

- Commands must end their name with `Command`
- Handler methods must be named `Execute`
- Handler methods must take one parameter, which is a `Command`
- Handler methods must return `CommandResult` or `Task<CommandResult>`

### Exposing Commands and Queries in a Web API

By deriving from the `EndpointGroup` class, you can expose a set of related commands and queries under a specific URL prefix. For example, the following exposes customer-related endpoints:

```csharp
public class CustomerEndpointGroup : EndpointGroup
{
    public CustomerEndpointGroup() : base("Customers")
    {
        CommandEndpoint<AddCustomerCommand>()
            .Post("")
            .AcceptedOnEvent<CustomerAddedEvent>("/customers/{Id}")
            .OtherwiseFail();

        CommandEndpoint<UpdateCustomerCommand>()
            .Put("")
            .AcceptedOnEvent<CustomerUpdatedEvent>("/customers/{Id}")
            .OtherwiseFail();

        QueryEndpoint<AllCustomersQuery>()
            .Get("/customers");

        QueryEndpoint<CustomerByIdQuery>()
            .Get("/customers/{Id}");
    }
}
```

This configuration will expose the following URLs in the Web API:

```
POST /customers - Expects an AddCustomerCommand object in the JSON body. Returns a 202 Accepted with a location header set to `/customers/{Id}`.
PUT /customers  - Expects an UpdateCustomerCommand object in the JSON body. Returns a 202 Accepted with a location header set to `/customers/{Id}`.
GET /customers  - Returns all customers. 200 OK when the query result is not null, and 404 Not Found when the result is null.
GET /customers/{Id}  - Returns a customer with the specified Id. 200 OK when the query result is not null, and 404 Not Found when the result is null.
```

EndpointGroups are automatically detected and registered by the Cyrus generators if they derive from the `EndpointGroup` class.

### Application Startup

Cyrus generates several extension methods to simplify application startup.

These methods are available on `IServiceCollection`:

- `AddTypedIds()` - Registers all generated strongly typed IDs and their associated `JsonConverters`
- `AddCqrs(Action<CqrsOptions> cfg)` - Registers all CommandHandlers, QueryHandlers, and supporting types. Optionally takes a lambda for custom configuration.

These methods are available on `IEndpointRouteBuilder`:

- `MapCqrs()` - Registers all endpoints from all known EndpointGroups.

A basic Cyrus application startup might look like this:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTypedIds();
builder.Services.AddCqrs(o => o.AddEndpointGroups());

var app = builder.Build();
app.MapCqrs();

await app.RunAsync();
```

Check the [demo solution](https://github.com/thuijer/CqrsGen/blob/master/) for more details.