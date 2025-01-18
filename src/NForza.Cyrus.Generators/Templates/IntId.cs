using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Serialization;
using NForza.Cyrus.Abstractions;

namespace % Namespace %;

[JsonConverter(typeof(% ItemName %JsonConverter))]
[TypeConverter(typeof(% ItemName %TypeConverter))]
[DebuggerDisplay("{Value}")]
public partial record struct % ItemName %(int Value): ITypedId
{
    % CastOperators %
    % IsValid %
    public override string ToString() => Value.ToString();
}