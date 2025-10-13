using NForza.Cyrus.Abstractions;

namespace TracksDemo.Tracks.Update;

public class UpdateTrackCommandHandler
{
    [CommandHandler(Route = "/tracks/{TrackId}", Verb = HttpVerb.Put)]
    public Result Update(UpdateTrackCommand command, Track? track)
    {
        if (track == null)
        {
            return new NotFoundResult(); // $"Track with Id {command.TrackId} not found.");
        }
        track.Artist = command.Artist;
        track.Title = command.Title;
        track.FileName = command.FileName;
        track.AudioFormat = command.AudioFormat;
        return new AcceptedResult();
    }
}
