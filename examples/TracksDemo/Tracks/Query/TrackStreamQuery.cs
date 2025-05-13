using CyrusDemo.Tracks.Create;
using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Query;

[Query]
public record TrackStreamQuery(TrackId TrackId, AudioFormat AudioFormat);