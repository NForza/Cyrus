﻿using CyrusDemo.Tracks.Create;

namespace CyrusDemo.Tracks;

public class Track(TrackId id, Title title, Artist artist, FileName fileName, AudioFormat audioFormat)
{
    public TrackId Id { get; set; } = id;
    public Title Title { get; set; } = title;
    public Artist Artist { get; set; } = artist;
    public FileName FileName { get; set; } = fileName;
    public AudioFormat AudioFormat { get; set; } = audioFormat;
}
