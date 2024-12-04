using System.Reflection;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NForza.Cyrus.Cqrs;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services
                .AddCqrs()
                .AddMassTransit( cfg =>
                {
                    cfg.AddConsumers(Assembly.GetExecutingAssembly());
                    cfg.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                })
            )
            .Build();

ICommandDispatcher commandDispatcher = host.Services.GetRequiredService<ICommandDispatcher>();
IQueryProcessor queryProcessor = host.Services.GetRequiredService<IQueryProcessor>();

CommandResult addResult = await commandDispatcher.Execute(new AddCustomerCommand(new("John Doe"), new("123 Main St")));
var customerId = addResult.Events.OfType<CustomerAddedEvent>().First().Id;

CommandResult updateResult = await commandDispatcher.Execute(new UpdateCustomerCommand(customerId, new("John Doe"), new("123 Main St")));
CommandResult deleteResult = await commandDispatcher.Execute(new DeleteCustomerCommand(customerId));

var customer = await queryProcessor.Query(new CustomerByIdQuery(customerId));
if (customer != null)
{
    Console.WriteLine( customer.Name );
}