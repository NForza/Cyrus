using DemoApp.Contracts;
using DemoApp.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NForza.Cqrs;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddCqrs())
            .Build();

ICommandDispatcher commandDispatcher = host.Services.GetRequiredService<ICommandDispatcher>();

CustomerIdTypeConverter nameTypeConverter = new();

Name name = Name.Empty;
if (name.IsNullOrEmpty())
{
    Console.WriteLine("Name is empty");
}

CustomerId customerId = CustomerId.Empty;
CommandResult addResult = await commandDispatcher.Execute(new AddCustomerCommand(new("John Doe"), new("123 Main St")));
CommandResult updateResult = await commandDispatcher.Execute(new UpdateCustomerCommand(new(), new("John Doe"), new("123 Main St")));
CommandResult deleteResult = await commandDispatcher.Execute(new DeleteCustomerCommand(new()));

