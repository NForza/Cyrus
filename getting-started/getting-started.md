# Cyrus - An Opinionated Framework for Creating CQRS Applications and Web APIs

## Goal

This small demo shows the core focus of Cyrus: to be able to quickly create WebApis with as little boilerplate code as possible. Cyrus uses C# Source Generators to accomplish this. It uses very little Reflection or lookups. Only at startup there is some Reflection code to add all the required services and to boot the application. When running, Cyrus uses no Reflection at all.

## Create your first Cyrus application

* Create a ASP.NET Core WebApi using .NET 9

* Remove the (outdated) `Swashbuckle.AspNetCore` Nuget package. Cyrus uses `Scalar` to visualise the OpenApi specification.

* Add the `NForza.Cyrus` Nuget package

* Replace the contents of Program.cs with the following:

```csharp
using NForza.Cyrus;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCyrus();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapCyrus();
app.Run();
```

* Note that the startup code is standard, except for the `AddCyrus` and `MapCyrus` methods.

* Verify that the application compiles and runs correctly

* Change the LaunchUrl from `weatherforcast` to `swagger` in LaunchSettings.json.

* Add a new c# file `Types.cs` to the project and add some strongly typed IDs:

```csharp
[GuidId]
public partial record struct CustomerId;

[StringId(1, 50)]
public partial record struct Name;

[StringId(1, 100)]
public partial record struct Address;
```

* In the same file (or a new if you prefer), add a command named NewCustomerCommand:

```csharp
[Command]
public record struct NewCustomerCommand(CustomerId CustomerId, Name Name, Address Address);
```

* Add a command handler:

```csharp
public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/customers", Verb = HttpVerb.Post)]
    public void Handle(NewCustomerCommand command)
    {
        Console.WriteLine("Creating a new customer");
    }
}
```

* Run the application and use the new endpoint from the `/swagger` page

* Note that the app has an endpoint with a Route of "/customers" and that is uses the POST verb as described by the CommandHandler attributes

* Verify that the command returns OK and that you see a message in the console

* Change the command handler to return an IResult:

```csharp
public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/customers", Verb = HttpVerb.Post)]
    public IResult Handle(NewCustomerCommand command)
    {
        Console.WriteLine("Creating a new customer");
        return Results.Accepted();
    }
}
```

* Run the application and use the updated endpoint from the `/swagger` page

* Verify that the command returns Accepted now and that you see a message in the console

* Note that this was the only change needed

* Add an event named CustomerCreatedEvent:

```csharp
[Event]
public record struct CustomerCreatedEvent(CustomerId CustomerId, Name Name, Address Address);
```

* Add an event handler:

```csharp
public class CustomerEventHandler
{
    [EventHandler]
    public void Handle(CustomerCreatedEvent customerCreatedEvent)
    {
        Console.WriteLine("A new customer has been created: " + customerCreatedEvent.CustomerId);
    }
}
```

* Update the CommandHandler to return a Tuple of an IResult and an IEnumerable of events:

```csharp
public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/customers", Verb = HttpVerb.Post)]
    public (IResult result, IEnumerable<object> events) Handle(NewCustomerCommand command)
    {
        Console.WriteLine("Creating a new customer");
        return (Results.Accepted(), [new CustomerCreatedEvent(command.CustomerId, command.Name, command.Address)]);
    }
}
```

* Run the application and observe that the endpoint still returns Accepted, but now the event handler is invoked as well and writes its message to the console. 

* Change the command handler to be a static method:

```csharp
public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/customers", Verb = HttpVerb.Post)]
    public static (IResult result, IEnumerable<object> events) Handle(NewCustomerCommand command)
    {
        Console.WriteLine("Creating a new customer");
        return (Results.Accepted(), [new CustomerCreatedEvent(command.CustomerId, command.Name, command.Address)]);
    }
}
```

* Run the application and note that it still runs as before.

* Change the command handler to be an async method:

```csharp
public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/customers", Verb = HttpVerb.Post)]
    public static async Task<(IResult result, IEnumerable<object> events)> HandleAsync(NewCustomerCommand command)
    {
        Console.WriteLine("Creating a new customer");
        await Task.Delay(100);
        return (Results.Accepted(), [new CustomerCreatedEvent(command.CustomerId, command.Name, command.Address)]);
    }
}
```

* Run the application and note that it still runs as before.

* Add a second command and command handler:

```csharp
[Command]
public record struct UpdateCustomerCommand(CustomerId CustomerId, Name Name, Address Address);

public class UpdateCustomerCommandHandler
{
    [CommandHandler(Route = "/{CustomerId:guid}", Verb = HttpVerb.Put)]
    public static IResult Update(UpdateCustomerCommand command)
    {
        Console.WriteLine("Updating customer");
        return Results.Accepted();
    }
}
```

* Run the application and open the `/swagger` page.

* Note that there is a new PUT endpoint that takes a `CustomerId` as a route parameter. This typed id is automatically exposed as a GUID.

* Note that the `CustomerId` is not a part of the contract of the object to be posted.

* Invoke the endpoint by supplying a Guid. In the debugger you can see that the posted JSON and the CustomerId from the route are merged into the UpdateCustomerCommand object that is received by the command handler. 

* Add a query and query handler:

```csharp
public record Customer(CustomerId Id, Name Name, Address Address);

[Query]
public record struct AllCustomersQuery;

public class CustomersQueryHandler
{
    [QueryHandler(Route = "/customers")]
    public static IEnumerable<Customer> Handle(AllCustomersQuery query)
    {
        Console.WriteLine("Getting all customers");
        return [new Customer(new CustomerId(), new Name("The Name"), new Address("The Address"))];
    }
};
```

* Run the application and open the `/swagger` page to see the new GET endpoint for the query.

* Add another query, and add a query handler to the existing CustomersQueryHandler class:

```csharp
[Query]
public record struct CustomerByIdQuery(CustomerId CustomerId);

public class CustomersQueryHandler
{
    [QueryHandler(Route = "/customers")]
    public static IEnumerable<Customer> GetAll(AllCustomersQuery query)
    {
        Console.WriteLine("Getting all customers");
        return [new Customer(new CustomerId(), new Name("The Name"), new Address("The Address"))];
    }

    [QueryHandler(Route = "/customers/{CustomerId}")]
    public async Task<Customer> GetById(CustomerByIdQuery query)
    {
        Console.WriteLine("Getting customer by Id: " + query.CustomerId);
        return new Customer(query.CustomerId, new Name("The Name"), new Address("The Address"));
    }
};
```

## Looking at the generated code

* In the csproj, add the following:

```xml
  <PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(MSBuildProjectDirectory)\obj\$(Configuration)\generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>
```

* After compiling in Debug configuration, the generated files can be found in the `obj\Debug\generated` in the project folder

## Next topics

* [Adding metadata to endpoints](adding-metadata.md)
* Adding validators for commands and queries
* Using MassTransit in Cyrus to broadcast events
* Splitting contracts and implementation
* Generating TypeScript from a Cyrus model
