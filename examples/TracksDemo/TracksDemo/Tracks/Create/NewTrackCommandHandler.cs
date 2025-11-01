using NForza.Cyrus.Abstractions;

namespace TracksDemo.Tracks.Create;

public class NewTrackCommandHandler(DemoContext context)
{
    [Validator]
    public static IEnumerable<string> Validate(NewTrackCommand command)
    {
        if (command.Title.IsEmpty())
        {
            yield return "Title cannot be empty.";
        }
        if (command.FileName.IsEmpty())
        {
            yield return "FileName cannot be empty.";
        }
    }

    [CommandHandler(Route = "/tracks", Verb = HttpVerb.Post)]
    public async Task<(Result Result, IEnumerable<object> Messages)> Handle(NewTrackCommand command)
    {
        Console.WriteLine("Creating a new track");
        Track c = new(command.TrackId, command.Title, command.Artist, command.FileName, command.AudioFormat);
        context.Tracks.Add(c);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (ArgumentException ae) when (ae.Message.Contains("same key"))
        {
           return (Result.Conflict($"Track with Id {command.TrackId} already exists."), []);
        }
        return (Result.Accepted(), [new TrackCreatedEvent(command.TrackId, command.Title, command.FileName)]);
    }
}
