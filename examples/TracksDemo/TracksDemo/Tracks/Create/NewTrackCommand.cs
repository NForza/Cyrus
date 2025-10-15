using NForza.Cyrus.Abstractions;

namespace TracksDemo.Tracks.Create;

[Command]
public record NewTrackCommand(TrackId TrackId, Title Title, Artist Artist, FileName FileName, AudioFormat AudioFormat);
