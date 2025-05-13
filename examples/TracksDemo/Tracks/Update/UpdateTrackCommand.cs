using CyrusDemo.Tracks.Create;
using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Update;

[Command]
public record struct UpdateTrackCommand(TrackId TrackId, Title Title, Artist Artist, FileName FileName, AudioFormat AudioFormat);
