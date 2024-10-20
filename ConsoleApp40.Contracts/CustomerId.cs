using NForza.TypedIds;

namespace DemoApp.Contracts;

public record CustomerId(int Value) : TypedId<int>(Value)
{
    public override bool IsValid() => Value > 0;
}
