using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Create;

[Command]
public record struct NewTrackCommand(TrackId TrackId, Title Title, Artist Artist, FileName FileName, AudioFormat AudioFormat);
