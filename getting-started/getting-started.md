# Create your first Cyrus application

* Create a ASP.NET Core WebApi

* Update all Nuget packages

* Add the NForza.Cyrus Nuget package

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
