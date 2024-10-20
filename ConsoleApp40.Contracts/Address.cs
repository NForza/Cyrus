using NForza.TypedIds;

namespace DemoApp.Contracts;

public record Address(string Value) : TypedId<string>(Value)
{
    public override bool IsValid() => !string.IsNullOrEmpty(Value);
}
