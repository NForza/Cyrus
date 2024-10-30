using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NForza.Cqrs;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddCqrs())
            .Build();

ICommandDispatcher commandDispatcher = host.Services.GetRequiredService<ICommandDispatcher>();

CommandResult addResult = await commandDispatcher.Execute(new AddCustomerCommand(new("John Doe"), new("123 Main St")));
CommandResult updateResult = await commandDispatcher.Execute(new UpdateCustomerCommand(new(), new("John Doe"), new("123 Main St")));
CommandResult deleteResult = await commandDispatcher.Execute(new DeleteCustomerCommand(new()));

IQueryProcessor queryProcessor = host.Services.GetRequiredService<IQueryProcessor>();
queryProcessor.Query(new AllCustomersQuery()); 
