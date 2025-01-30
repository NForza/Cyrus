using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Cqrs.WebApi;

public class SignalREvent
{
    public INamedTypeSymbol Symbol { get; set; } = null!;
    public string MethodName { get; internal set; } = string.Empty;
    public bool IsBroadcast { get; set; }
}
