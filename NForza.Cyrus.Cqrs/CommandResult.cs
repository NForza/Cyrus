using System.Collections.Generic;
using System.Linq;

namespace NForza.Cyrus.Cqrs;

public class CommandResult
{
    public static CommandResult CompletedSuccessfully => new();
    public static CommandResult FailedWithError(params CommandError[] commandErrors)
    {
        CommandResult result = new();
        result.AddErrors(commandErrors);
        return result;
    }

    public CommandResult()
    {        
    }

    public CommandResult(params object[] events) => this.events = events.ToList();

    public static CommandResult FailedWithError(string errorKey, string errorDescription) => FailedWithError(new CommandError(errorKey, errorDescription));
    public static CommandResult ReturnEvents(params object[] events) => new CommandResult() {};
    private readonly List<CommandError> errorMessages = [];
    private readonly List<object> events = [];

    public bool Succeeded => !errorMessages.Any();

    public IEnumerable<CommandError> Errors => errorMessages.AsReadOnly();

    public IEnumerable<object> Events => events.AsReadOnly();

    public bool HasEvent<T>() => events.OfType<T>().Any();

    public void AddError(CommandError commandError)
    {
        errorMessages.Add(commandError);
        events.Clear();
    }

    public void AddError(string errorKey, string errorDescrption) => AddError(new CommandError(errorKey, errorDescrption));

    public void AddErrors(IEnumerable<CommandError> messages)
    {
        foreach (var message in messages)
        {
            AddError(message);
        }
    }

    public void AddEvent(object @event)
    {
        if (!errorMessages.Any())
            events.Add(@event);
    }

    public void AddEvents(IEnumerable<object> events)
    {
        foreach (var @event in events)
        {
            AddEvent(@event);
        }
    }
}
