using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NForza.Cyrus.Abstractions;

namespace % Namespace %;

[JsonConverter(typeof(% ItemName %JsonConverter))]
[TypeConverter(typeof(% ItemName %TypeConverter))]
[DebuggerDisplay("{Value}")]
public partial record struct % ItemName %(string Value): ITypedId
{
    public static % ItemName % Empty => new % ItemName %(string.Empty);
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
    % CastOperators %
    % IsValid %
    public override string ToString() => Value?.ToString();
}
