using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Aggregates;

namespace TracksDemo.Tracks.Update;

[Command]
public record UpdateTrackCommand([property: AggregateRootId] TrackId TrackId, Title Title, Artist Artist, FileName FileName, AudioFormat AudioFormat);
