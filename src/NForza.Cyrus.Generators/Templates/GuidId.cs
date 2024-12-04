using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Serialization;
using NForza.Cyrus.TypedIds;

namespace % Namespace %;

[JsonConverter(typeof(% ItemName %JsonConverter))]
[TypeConverter(typeof(% ItemName %TypeConverter))]
[DebuggerDisplay("{Value}")]
public partial record struct % ItemName %(Guid Value): ITypedId, IComparable<% ItemName %>, IComparable
{
    % Constructor %
    public static % ItemName % Empty => new % ItemName %(% Default %);
    % CastOperators %
    public override string ToString() => Value.ToString();
    public int CompareTo(% ItemName % other) => Value.CompareTo(other.Value);
    public int CompareTo(object obj) => obj is % ItemName % other ? CompareTo(other) : throw new ArgumentException("Object is not a % ItemName %");
}