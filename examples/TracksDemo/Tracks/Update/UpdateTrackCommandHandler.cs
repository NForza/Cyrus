using NForza.Cyrus.Abstractions;
using TracksDemo.Tracks;

namespace TracksDemo.Tracks.Update;

public class UpdateTrackCommandHandler(DemoContext context)
{
    [CommandHandler(Route = "/tracks/{TrackId}", Verb = HttpVerb.Put)]
    public IResult Update(UpdateTrackCommand command, Track? track)
    {
        if (track == null)
        {
            return Results.NotFound($"Track with Id {command.TrackId} not found.");
        }
        track.Artist = command.Artist;
        track.Title = command.Title;
        track.FileName = command.FileName;
        track.AudioFormat = command.AudioFormat;
        return Results.Accepted();
    }
}
