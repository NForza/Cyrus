namespace NForza.Cyrus.SignalR;

public class SignalRHub
{
    public void UsePath(string path) { }
    public ISignalRQueryBuilder<T> Query<T>() 
        => new SignalRQueryBuilder<T>();

    public ISignalRCommandBuilder<T> Command<T>() 
        => new SignalRCommandBuilder<T>();

    public void PublishesEventToCaller<T>() {  }
    public void PublishesEventToAll<T>() { }
}