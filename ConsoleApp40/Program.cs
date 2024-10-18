using ConsoleApp40.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NForza.Cqrs;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddCqrs())
            .Build();

var commandDispatcher = host.Services.GetRequiredService<ICommandDispatcher>();

CommandResult addResult = await commandDispatcher.Execute(new AddCustomerCommand("John Doe", "123 Main St"));

CommandResult deleteResult = await commandDispatcher.Execute(new DeleteCustomerCommand(42));

