using NForza.Cyrus.Abstractions;
using TracksDemo.Tracks;

namespace TracksDemo.Tracks.Query;

[Query]
public record struct TrackInfoByIdQuery(TrackId TrackId);
