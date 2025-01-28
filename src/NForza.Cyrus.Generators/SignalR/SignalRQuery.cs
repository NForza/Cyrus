using NForza.Cyrus.Generators.SignalR;

namespace NForza.Cyrus.Generators.Cqrs.WebApi;

public class SignalRQuery
{
    public string Name { get; internal set; } = string.Empty;
    public string FullTypeName { get; internal set; } = string.Empty;
    public string MethodName { get; internal set; } = string.Empty;
    public SignalRQueryReturnType ReturnType { get; internal set; } = new(null, false, false);
}
