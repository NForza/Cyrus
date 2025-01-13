namespace NForza.Cyrus.SignalR;

public class SignalRHub
{
    public ISignalRQueryBuilder<T> QueryMethodFor<T>() => new SignalRQueryBuilder<T>();
    public ISignalRCommandBuilder<T> CommandMethodFor<T>() => new SignalRCommandBuilder<T>();
}