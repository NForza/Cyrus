using System.Collections;

namespace NForza.Cyrus.Cqrs;

public class CommandDispatcher(IEnumerable<IEventBus> eventBuses, ICommandBus commandBus) : ICommandDispatcher
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
        Parallel.ForEach(eventBuses, async eventBus =>
        {
            foreach (var e in events)
            {
                await eventBus.Publish(e);
            }
        });
    }
}