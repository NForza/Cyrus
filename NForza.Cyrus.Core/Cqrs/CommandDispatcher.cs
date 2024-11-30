using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace NForza.Cyrus.Cqrs;

public class CommandDispatcher(IEventBus eventBus, ICommandBus commandBus) : ICommandDispatcher
{
    public async Task<CommandResult> ExecuteInternalAsync(object command, CancellationToken cancellationToken)
    {
        CommandResult result = await commandBus.Execute(command, cancellationToken);
        if (result.Succeeded)
            await DispatchEvents(result.Events);
        return result;
    }

    public CommandResult ExecuteInternalSync(object command)
    {
        CommandResult result = commandBus.Execute(command, CancellationToken.None).Result;
        if (result.Succeeded)
            DispatchEvents(result.Events).Wait();
        return result;
    }

    private async Task DispatchEvents(IEnumerable events)
    {
        foreach (var e in events)
        {
            await eventBus.Publish(e);
        }
    }
}