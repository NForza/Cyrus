using NForza.Cyrus.Abstractions;

namespace TracksDemo.Tracks.Query;

[Query]
public record TrackStreamQuery(TrackId TrackId, AudioFormat AudioFormat);