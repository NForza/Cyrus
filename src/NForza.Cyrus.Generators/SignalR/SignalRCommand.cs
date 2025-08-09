using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.SignalR;

public class SignalRCommand
{
    public string Name { get; internal set; } = string.Empty;
    public string ClrTypeName { get; internal set; } = string.Empty;
    public string MethodName { get; internal set; } = string.Empty;
    public IMethodSymbol? Handler { get; internal set; }
}
