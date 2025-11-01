using NForza.Cyrus.Abstractions;

namespace TracksDemo.Tracks.Create;

[Event]
public record TrackCreatedEvent(TrackId TrackId, Title Name, FileName Address);
