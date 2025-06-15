<img src="images/cyrus.png" alt="Cyrus" width="250" />

# Cyrus - An Opinionated Framework for Creating CQRS Applications and Web APIs

Cyrus is a CQRS framework that focuses on ease of use, simplicity, and minimizing boilerplate code. It incorporates principles from [DDD](https://en.wikipedia.org/wiki/Domain-driven_design) and [Cqrs](https://en.wikipedia.org/wiki/Command_Query_Responsibility_Segregation) to enable the rapid and pragmatic creation of Web APIs. Cyrus leverages [Roslyn Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md) to generate code rather than relying on reflection.

Cyrus generates code for the following components:

- [Custom Value Types](#custom-value-types)
- [Queries and Query Handlers](#queries-and-query-handlers)
- [Commands and Command Handlers](#commands-and-command-handlers)
- [Exposing Commands and Queries in a Web API](#exposing-commands-and-queries-in-a-web-api)
- [Application Startup](#application-startup)

### Custom Value Types

Cyrus allows you to define custom value types by defining a `partial record struct` like this:

```csharp
[StringValue(3, 200)]
public partial record struct Address;

[IntValue]
public partial record struct Amount;

[GuidValue]
public partial record struct CustomerId;
```
These becomes a new datatype that can be used in type definitions:

```csharp
public record Customer(CustomerId Id, Address Address);
```

### Queries and Query Handlers

Query Handlers process queries to retrieve data. A Query object is defined as a simple `record` or `record struct`:

```csharp
[Query]
public record struct CustomerByIdQuery(CustomerId Id);
```

A handler is responsible for handling a request for data. A simplified handler for this query might look like this:

```csharp
public class CustomersQueryHandler
{
    [QueryHandler]
    public static Customer Query(CustomerByIdQuery query)
    {
        return new Customer(query.Id, new($"Customer-{query.Id}"));
    }
}
```

Query handlers can return any type of object, and can be either static or instance methods. Dependency injection is supported and query handlers can be synchronous or asynchronous. Here are valid variations of the query handler method:

```csharp
public Customer Query(CustomerByIdQuery query)
public static Customer Query(CustomerByIdQuery query)
public static async Task<Customer> Query(CustomerByIdQuery query)
public async Task<Customer> Query(CustomerByIdQuery query)
```

No extra configuration is needed to hook up queries and their handlers. The code generation connects queries to handlers based on these requirements:

- A Query type must have the `[Query]` attribute 
- Handler methods must have the `[QueryHandler]` attribute
- Handler methods must take one parameter, which should be a `Query` type

Dependency injection is supported for instance methods, both synchronous and asynchronous.

### Commands and Command Handlers

Commands and their handlers are structured similarly to queries. Command are required to make changes to the domain and generate zero or more events inform for other parts of the solution of these changes. For example:

```csharp
[Command]
public record struct AddCustomerCommand(Name Name, Address Address);

public class AddCustomerCommandHandler
{
    [CommandHandler]
    public IEnumerable<object> Execute(AddCustomerCommand command)
    {
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        return [new CustomerAddedEvent(new CustomerId(), command.Name, command.Address)];
    }
}
```

Command handlers must return one of the following:
| return type                                     | effects after executing handler                                                                           |
|-------------------------------------------------|----------------------------------------------------------------------------------------------------------|
| void                                            | no return value or events                                                                                |
| `IEnumerable<object>`                           | no return value, but returned messages (events or commands) are published to the event or command bus    |
| `(object Result, IEnumerable<object> events)`   | return value and messages (events or commands) that are published to the event or command bus            |
| `(IResult Result, IEnumerable<object> events)`  | WebApi result and messages (events or commands) that are published to the event or command bus           |
| `(IResult Result, object message)`              | WebApi result and a single message (events or command) that is published to the event or command bus     |

Returned messages are dispatched through the system via an event or command bus, and you can use a local bus (which is the default) or integrate with an external event bus using [MassTransit](https://masstransit.io/).

The requirements for command handlers are as follows:

- Commands must have the `[Command]` attribute
- Handler methods must be have the `[CommandHandler]` attribute
- Handler methods must take one parameter, which is a `Command`
- Handler methods must return one of the mentioned return types

### Events and Event Handlers

Events and their handlers are structured similarly to commands and queries. Events inform for other parts of the solution of changes. 

```csharp
[Event]
public record CustomerAddedEvent(CustomerId Id, Name Name, Address Address);

public class CustomerEventHandler
{
    [EventHandler]
    public void Handle(CustomerAddedEvent @event)
    {
        Console.WriteLine($"Customer Added: {@event.Id}");
    }
}
```

Event handlers must return `void`. The requirements for command handlers are as follows:

- Events must have the `[Event]` attribute
- Handler methods must have the `[EventHandler]` attribute
- Handler methods must take one parameter, which is an `Event`
- Handler methods must return `void` or `Task`

### Exposing Commands and Queries in a Web API

By adding a Route and Verb to the CommandHandler or a Route to a QueryHandler, commands and queries can be exposed in a Web API:

```csharp
[CommandHandler(Route = "customers", Verb = HttpVerb.Post)]
public void AddCustomerCommandHandler(AddCustomerCommand command) { ... }

[QueryHandler(Route = "/customers/{page}/{pageSize}")]
public IEnumerable<Command> AllCustomersQueryHandler(AllCustomersQuery query) { ... }
```

### Application Startup

Cyrus just needs a few simple calls in the Program.cs to hook up all Commands, Queries and events:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCyrus();

var app = builder.Build();

app.MapCyrus();

await app.RunAsync();
```
Check the [Getting Started](https://github.com/NForza/Cyrus/tree/main/examples/GettingStarted/SimpleCyrusWebApi)) for more details.

### Under the hood

Cyrus uses a small amount of Reflection when the app is starting up, but **none** when the application is running. Cyrus generates a lot of source code at compile time to glue all the different parts together using [C# Source Generators]. This greatly simplifies development by reducing boilerplate code.

### Generated code for Custom Value Types

For every Custom Value Type Cyrus generates additional code that simplifies using this type in your code, including a custom `JsonConverter`, a `TypeConverter`, casting operators, validation and other methods.

```csharp
[JsonConverter(typeof(AddressJsonConverter))]
[TypeConverter(typeof(AddressTypeConverter))]
public partial record struct Address(string Value) : ICustomValueType
{
    public static Address Empty => new Address(string.Empty);
    
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
    
    public static implicit operator string(DemoApp.Contracts.Address valueType) => valueType.Value;
    public static explicit operator DemoApp.Contracts.Address(string value) => new(value);
    
    public bool IsValid() => !string.IsNullOrEmpty(Value) && Value.Length >= 3 && Value.Length <= 200;
    
    public override string ToString() => Value?.ToString();
}
```

For GUID-based Custom Value Types, you can use the `GuidValue` attribute:

```csharp
[GuidValue]
public partial record struct CustomerId;
```

The generated code for a `Guid`-based value type looks like this:

```csharp
[JsonConverter(typeof(CustomerIdJsonConverter))]
[TypeConverter(typeof(CustomerIdTypeConverter))]
public partial record struct CustomerId(Guid Value) : ICustomValueType
{
    public CustomerId() : this(Guid.NewGuid()) { }
    public static CustomerId Empty => new CustomerId(Guid.Empty);
    
    public static implicit operator System.Guid(DemoApp.Contracts.CustomerId valueType) => valueType.Value;
    public static explicit operator DemoApp.Contracts.CustomerId(System.Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}
```

## Why Cyrus?

The name stems from [Cyrus The Great](https://en.wikipedia.org/wiki/Cyrus_the_Great), who was considered a great commander, but also a great sage.
