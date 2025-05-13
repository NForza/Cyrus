using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Query;

[Query]
public record struct TrackInfoByIdQuery(TrackId TrackId);
