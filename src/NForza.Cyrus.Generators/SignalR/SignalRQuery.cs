using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.SignalR;

namespace NForza.Cyrus.Generators.Cqrs.WebApi;

public class SignalRQuery
{
    public INamedTypeSymbol Symbol { get; set; } = null!;
    public string MethodName { get; internal set; } = string.Empty;
    public SignalRQueryReturnType ReturnType { get; internal set; } = new(null, false, false);
}
