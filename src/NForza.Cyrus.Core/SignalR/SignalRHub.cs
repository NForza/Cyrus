namespace NForza.Cyrus.SignalR;

public class SignalRHub
{
    public void UsePath(string path) { }
    public ISignalRQueryBuilder<T> QueryMethodFor<T>() => new SignalRQueryBuilder<T>();
    public ISignalRCommandBuilder<T> CommandMethodFor<T>(bool replyToAllClients = false) => new SignalRCommandBuilder<T>();
    public void EventToCaller<T>() { }
    public void EventToAll<T>() { }
}