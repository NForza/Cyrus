namespace NForza.Cyrus.SignalR;

public class SignalRHub
{
    public List<Type> UsedTypes = [];
    public void UsePath(string path) { }
    public ISignalRQueryBuilder<T> QueryMethodFor<T>()
    {
        UsedTypes.Add(typeof(T));
        return new SignalRQueryBuilder<T>();
    }

    public ISignalRCommandBuilder<T> CommandMethodFor<T>(bool replyToAllClients = false)
    {
        UsedTypes.Add(typeof(T));
        return new SignalRCommandBuilder<T>();
    }

    public void EventToCaller<T>() { UsedTypes.Add(typeof(T)); }
    public void EventToAll<T>() { UsedTypes.Add(typeof(T)); }
}