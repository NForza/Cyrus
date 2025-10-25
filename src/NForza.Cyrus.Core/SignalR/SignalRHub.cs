namespace NForza.Cyrus.SignalR;

public class CyrusSignalRHub
{
    public void UsePath(string path) { }
    public ISignalRQueryBuilder<T> Expose<T>()
        => new SignalRQueryBuilder<T>();

    public void Emit<T>() { }
    public void Broadcast<T>() { }
}