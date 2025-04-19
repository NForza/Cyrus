namespace NForza.Cyrus.SignalR;

public class CyrusSignalRHub
{
    public void UsePath(string path) { }
    public ISignalRQueryBuilder<T> Expose<T>() 
        => new SignalRQueryBuilder<T>();

    public void Send<T>() {  }
    public void Broadcast<T>() { }
}