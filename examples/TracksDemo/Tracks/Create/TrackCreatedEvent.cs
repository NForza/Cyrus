using NForza.Cyrus.Abstractions;
using TracksDemo.Tracks;

namespace TracksDemo.Tracks.Create;

[Event]
public record TrackCreatedEvent(TrackId TrackId, Title Name, FileName Address);
