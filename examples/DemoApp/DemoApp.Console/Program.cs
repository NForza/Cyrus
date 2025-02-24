using System.Reflection;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NForza.Cyrus;
using NForza.Cyrus.Cqrs;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services
                .AddCyrus()
                .AddMassTransit(cfg =>
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

CommandResult addResult = await commandDispatcher.Execute(new AddCustomerCommand(new("John Doe"), new(new("Main St"), new(123)), CustomerType.Private));
var customerId = addResult.Events.OfType<CustomerAddedEvent>().First().Id;

CommandResult updateResult = await commandDispatcher.Execute(new UpdateCustomerCommand(customerId, new("John Doe"), new(new("Main St"), new(12))));
CommandResult deleteResult = await commandDispatcher.Execute(new DeleteCustomerCommand(customerId));

var customer = await queryProcessor.Query(new CustomerByIdQuery(customerId));
if (customer != null)
{
    Console.WriteLine(customer.Name);
}