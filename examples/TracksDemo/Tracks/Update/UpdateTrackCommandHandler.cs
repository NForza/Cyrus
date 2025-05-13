using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Update;

public class UpdateTrackCommandHandler(DemoContext context)
{
    [CommandHandler(Route = "/tracks/{TrackId}", Verb = HttpVerb.Put)]
    public IResult Update(UpdateTrackCommand command)
    {
        Console.WriteLine("Updating track");
        Track? track = context.Tracks.FirstOrDefault(c => c.Id == command.TrackId);
        if (track == null)
        {
            return Results.NotFound($"Track with Id {command.TrackId} not found.");
        }
        track.Artist = command.Artist;
        track.Title = command.Title;
        track.FileName = command.FileName;
        track.AudioFormat = command.AudioFormat;
        context.Tracks.Update(track);
        context.SaveChangesAsync();
        return Results.Accepted();
    }
}
