namespace NForza.Cyrus.SignalR;

public class SignalRHub
{
    public void UsePath(string path) { }
    public ISignalRQueryBuilder<T> QueryMethodFor<T>() 
        => new SignalRQueryBuilder<T>();

    public ISignalRCommandBuilder<T> CommandMethodFor<T>(bool replyToAllClients = false) 
        => new SignalRCommandBuilder<T>();

    public void PublishesEventToCaller<T>() {  }
    public void PublishesEventToAll<T>() { }
}