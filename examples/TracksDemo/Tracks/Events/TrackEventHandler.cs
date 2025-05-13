using CyrusDemo.Tracks.Create;
using NForza.Cyrus.Abstractions;

namespace CyrusDemo.Tracks.Events;

public class TrackEventHandler
{
    [EventHandler]
    public void Handle(TrackCreatedEvent trackCreatedEvent)
    {
        Console.WriteLine("A new track has been created: " + trackCreatedEvent.TrackId);
    }
}
