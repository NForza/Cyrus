namespace NForza.Cyrus.SignalR;

public class SignalRHubDictionary : Dictionary<string, Type>
{
    public void AddSignalRHub<T>(string path)
    {
        Add(path, typeof(T));
    }
}
