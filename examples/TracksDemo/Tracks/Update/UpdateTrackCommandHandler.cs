using NForza.Cyrus.Abstractions;
using TracksDemo.Tracks;

namespace TracksDemo.Tracks.Update;

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

    public IResult Update2(UpdateTrackCommand command, Track track)
    {
        Console.WriteLine("Updating track");
        track.Artist = command.Artist;
        track.Title = command.Title;
        track.FileName = command.FileName;
        track.AudioFormat = command.AudioFormat;
        return Results.Accepted();
    }

}
