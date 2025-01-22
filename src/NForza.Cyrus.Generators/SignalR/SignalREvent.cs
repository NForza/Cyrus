namespace NForza.Cyrus.Generators.Cqrs.WebApi;

public class SignalREvent
{
    public string Name { get; internal set; } = string.Empty;
    public string FullTypeName { get; internal set; } = string.Empty;
    public string MethodName { get; internal set; } = string.Empty;
    public bool IsBroadcast { get; set; }
}
