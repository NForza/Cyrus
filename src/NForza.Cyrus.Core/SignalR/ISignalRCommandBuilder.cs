namespace NForza.Cyrus.SignalR;


public interface ISignalRCommandBuilder<T>
{
    void ReplyToAllClients();
}