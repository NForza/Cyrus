using Microsoft.EntityFrameworkCore;
using NForza.Cyrus.Abstractions;
using TracksDemo.Tracks.Query;

namespace CyrusDemo.Tracks.Query;

public class TracksQueryHandler(DemoContext context)
{
    [QueryHandler(Route = "/tracks")]
    public async Task<IEnumerable<Track>> GetAll(AllTracksQuery query)
    {
        Console.WriteLine("Getting all tracks");
        return await context.Tracks.ToListAsync();
    }

    [QueryHandler(Route = "/tracks/{TrackId}")]
    public async Task<Track?> GetById(TrackInfoByIdQuery query)
    {
        Console.WriteLine("Getting track by Id: " + query.TrackId);
        return await context.Tracks.FirstOrDefaultAsync(c => c.Id == query.TrackId);
    }

    [QueryHandler(Route = "/tracks/{TrackId}/{AudioFormat}")]
    public async Task<(Stream?, string?)> GetById(TrackStreamQuery query)
    {
        Console.WriteLine("Getting track by Id: " + query.TrackId);
        var track = await context.Tracks.FirstOrDefaultAsync(c => c.Id == query.TrackId && c.AudioFormat == query.AudioFormat);
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

    [QueryHandler(Route = "/tracks/search/{SearchTerms}")]
    public async Task<IEnumerable<Track>> Search(SearchTracksQuery query)
    {
        Console.WriteLine("Searching tracks: " + query.SearchTerms);
        var searchTerms = query.SearchTerms.ToLower().Split(' ');
        return context.Tracks
            .Where(c => searchTerms.Any(term => 
                c.Title.ToString().Contains(term, StringComparison.CurrentCultureIgnoreCase) 
                ||
                c.Artist.ToString().Contains(term, StringComparison.CurrentCultureIgnoreCase)))
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();
    }
};
