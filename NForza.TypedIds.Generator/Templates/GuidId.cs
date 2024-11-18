using System;
using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Serialization;
using NForza.TypedIds;

namespace % Namespace %;

[JsonConverter(typeof(% ItemName %JsonConverter))]
[TypeConverter(typeof(% ItemName %TypeConverter))]
public partial record struct % ItemName %(Guid Value): ITypedId
{
    % Constructor %
    public static % ItemName % Empty => new % ItemName %(% Default %);
    % CastOperators %
    public override string ToString() => Value.ToString();
}