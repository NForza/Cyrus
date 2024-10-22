using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace NForza.Cqrs;

public class CommandDispatcher(IEventBus eventBus, ICommandBus commandBus) : ICommandDispatcher
{
    public async Task<CommandResult> ExecuteInternal(object command, CancellationToken cancellationToken)
    {
        CommandResult result = await commandBus.Execute(command, cancellationToken);
        if (result.Succeeded)
            await DispatchEvents(result.Events);
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