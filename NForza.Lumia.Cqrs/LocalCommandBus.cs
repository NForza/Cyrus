using System;
using System.Threading;
using System.Threading.Tasks;

namespace NForza.Lumia.Cqrs;

public class LocalCommandBus(IServiceProvider serviceProvider, CommandHandlerDictionary handlers ) : ICommandBus
{
    public Task<CommandResult> Execute(object command, CancellationToken cancellationToken)
    {            
        var handler = handlers[command.GetType()] ?? throw new InvalidOperationException($"{command.GetType().Name} has no registered handler");
        return handler(serviceProvider, command);
    }
}
