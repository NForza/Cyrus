using NForza.Cyrus.Abstractions;
using TracksDemo.Tracks.Create;

namespace TracksDemo.Tracks.Events;

public class TrackEventHandler
{
    [EventHandler]
    public void Handle(TrackCreatedEvent trackCreatedEvent)
    {
        Console.WriteLine("A new track has been created: " + trackCreatedEvent.TrackId);
    }
}
