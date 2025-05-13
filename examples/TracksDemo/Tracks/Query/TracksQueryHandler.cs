using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Query;

public class TracksQueryHandler(DemoContext context)
{
    [QueryHandler(Route = "/tracks")]
    public IEnumerable<Track> GetAll(AllTracksQuery query)
    {
        Console.WriteLine("Getting all tracks");
        return context.Tracks.ToList();
    }

    [QueryHandler(Route = "/tracks/{TrackId}")]
    public async Task<Track?> GetById(TrackInfoByIdQuery query)
    {
        Console.WriteLine("Getting track by Id: " + query.TrackId);
        return context.Tracks.FirstOrDefault(c => c.Id == query.TrackId);
    }

    [QueryHandler(Route = "/tracks/{TrackId}/{AudioFormat}")]
    public async Task<(Stream?, string?)> GetById(TrackStreamQuery query)
    {
        Console.WriteLine("Getting track by Id: " + query.TrackId);
        var track = context.Tracks.FirstOrDefault(c => c.Id == query.TrackId && c.AudioFormat == query.AudioFormat);
        if (track == null)
        {
            return (null, null);
        }
        var filePath = Path.Combine("Content", track.AudioFormat.ToString(), track.FileName.ToString());
        if (!File.Exists(filePath))
        {
            return (null, null);
        }
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return (stream, "audio/mpeg");
    }
};
