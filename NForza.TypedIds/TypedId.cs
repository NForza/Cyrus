using System.ComponentModel;

namespace NForza.TypedIds;

[TypeConverter(typeof(TypedIdConverter))]
public abstract record TypedId<TValue>(TValue Value)
    where TValue : notnull
{
    public override string? ToString() => Value.ToString();
    public virtual bool IsValid() => true;
}
