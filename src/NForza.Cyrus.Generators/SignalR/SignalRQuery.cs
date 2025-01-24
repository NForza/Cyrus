namespace NForza.Cyrus.Generators.Cqrs.WebApi;

public class SignalRQuery
{
    public string Name { get; internal set; } = string.Empty;
    public string FullTypeName { get; internal set; } = string.Empty;
    public string MethodName { get; internal set; } = string.Empty;
    public string ReturnType { get; internal set; } = string.Empty;
    public bool ReturnsCollection { get; set; } = false;
}
