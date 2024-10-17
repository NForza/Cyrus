using ConsoleApp40.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NForza.Cqrs;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddCqrs();
            })
            .Build();

var commandProcessor = host.Services.GetRequiredService<ICommandProcessor>();

int result = commandProcessor.Execute(new AddCustomerCommand("John Doe", "123 Main St"));

bool success = commandProcessor.Execute(new DeleteCustomerCommand(42));