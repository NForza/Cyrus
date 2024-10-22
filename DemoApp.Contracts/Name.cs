using NForza.TypedIds;

namespace DemoApp.Contracts;

[TypedId]
public partial record struct Name(string Value)
{
    public bool IsValid() => Value.Length > 0;
}
