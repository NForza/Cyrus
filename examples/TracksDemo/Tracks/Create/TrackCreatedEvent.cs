using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Create;

[Event]
public record TrackCreatedEvent(TrackId TrackId, Title Name, FileName Address);
