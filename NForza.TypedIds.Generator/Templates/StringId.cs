using System;
using NForza.TypedIds;
using System.Text.Json.Serialization;

namespace % Namespace %;

[JsonConverter(typeof(% ItemName %JsonConverter))]
public partial record struct % ItemName %(string Value): ITypedId
{
    public static % ItemName % Empty => new % ItemName %(string.Empty);
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
    % CastOperators %
    % IsValid %
    public override string ToString() => Value.ToString();
}
