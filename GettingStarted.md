# Building your first Cyrus web api

1. Create a new **ASP.NET Core Web API** 

2. Update all packages

3. Add the `NForza.Cyrus` Nuget package to the application

`dotnet add package NForza.Cyrus`

4. Add a new strongly typed ID:
```csharp
using NForza.Cyrus.TypedIds;

namespace CyrusGettingStarted;

[GuidId]
public partial record struct CustomerId;
```

5. Add a Query definition

```csharp
namespace CyrusGettingStarted;

public record struct CustomerByIdQuery(CustomerId Id);
```

6. Add a handler for the Query:

```csharp
namespace CyrusGettingStarted;

public static class CustomerByIdQueryHandler
{
    public static string Query(CustomerByIdQuery query)
    {
        return $"Customer {query.Id}";
    }
}
```

7. Add a EndpointGroup:

```csharp
using NForza.Cyrus.WebApi;

namespace CyrusGettingStarted;

public class CustomerEndpointGroup : EndpointGroup
{
    public CustomerEndpointGroup() : base("customers")
    {
        QueryEndpoint<CustomerByIdQuery>()
            .Get("{Id}");
    }
}

```

8. Modify the Program.cs

```csharp
using NForza.Cyrus.TypedIds;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTypedIds();
builder.Services.AddCqrs(o => o.AddEndpointGroups());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapCqrs();

app.Run();
```

9. Run the application.