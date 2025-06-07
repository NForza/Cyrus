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

public static class HelloCommandHandler
{
    [CommandHandler(Route = "hello-cyrus")]
    public static void HandleAsync(HelloCommand command)
    {
        Console.WriteLine("Hello, Cyrus!");
    }
}

[Query]
public partial record struct AreYouOkQuery;

public static class AreYouOkQueryHandler
{
    [QueryHandler(Route = "are-you-ok")]
    public static string HandleAsync(AreYouOkQuery query)
    {
        return "yes";
    }
}


