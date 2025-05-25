using NForza.Cyrus.Abstractions;

namespace TracksDemo.Tracks.Update;

[Command]
public record struct UpdateTrackCommand(TrackId TrackId, Title Title, Artist Artist, FileName FileName, AudioFormat AudioFormat);
