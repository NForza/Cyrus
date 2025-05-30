using NForza.Cyrus.Abstractions;
using TracksDemo;
using TracksDemo.Tracks;

namespace TracksDemo.Tracks.Create;

public class NewTrackCommandHandler(DemoContext context)
{
    [Validator]
    public static IEnumerable<string> Validate(NewTrackCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            yield return "Name cannot be empty.";
        }
        if (string.IsNullOrWhiteSpace(command.FileName))
        {
            yield return "Address cannot be empty.";
        }
    }

    [CommandHandler(Route = "/tracks", Verb = HttpVerb.Post)]
    public async Task<(IResult Result, IEnumerable<object> Messages)> Handle(NewTrackCommand command)
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
           return (Results.Conflict($"Track with Id {command.TrackId} already exists."), []);
        }
        return (Results.Accepted(), [new TrackCreatedEvent(command.TrackId, command.Title, command.FileName)]);
    }
}
