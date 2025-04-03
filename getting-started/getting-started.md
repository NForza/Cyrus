# Create your first Cyrus application

* Create a ASP.NET Core WebApi

* Update all Nuget packages

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

* Verify that the application compiles and runs

* Add some strongly typed IDs:

```csharp
[GuidId]
public partial record struct CustomerId;

[StringId(1, 50)]
public partial record struct Name;

[StringId(1, 100)]
public partial record struct Address;
```

* Add a command named NewCustomerCommand:

```csharp
[Command(Route = "/", Verb = HttpVerb.Post)]
public record struct NewCustomerCommand(CustomerId CustomerId, Name Name, Address Address);
```

* Add an event named CustomerCreatedEvent:

```csharp
[Event]
public record struct CustomerCreatedEvent(CustomerId CustomerId, Name Name, Address Address);
```

* Add a command handler:

```csharp
public class NewCustomerCommandHandler
{
    [CommandHandler]
    public async Task<(IResult result, IEnumerable<object> events)> Handle(NewCustomerCommand command)
    {
        var customerCreatedEvent = new CustomerCreatedEvent(command.CustomerId, command.Name, command.Address);
        Console.WriteLine("Creating a new customer");
        return (Results.Ok(), [customerCreatedEvent]);
    }
}
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

* Run the application and observe that:

    * The app has an endpoint with a Route of "/" and that is uses the POST verb
    * The endpoint returns a 200 OK as indicated by the Command
    * When the endpoint is invoked, the event handler is triggered (output is on the console)