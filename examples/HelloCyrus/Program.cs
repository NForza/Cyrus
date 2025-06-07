using NForza.Cyrus;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCyrus();

var app = builder.Build();

app.MapCyrus();

await app.RunAsync();

[Command]
public partial record struct HelloCommand;

public partial class HelloCommandHandler
{
    [CommandHandler(Route = "hello-cyrus")]
    public ValueTask HandleAsync(HelloCommand command)
    {
        Console.WriteLine("Hello, Cyrus!");
        return ValueTask.CompletedTask;
    }
}


