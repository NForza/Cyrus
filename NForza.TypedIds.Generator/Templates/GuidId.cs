using System;
using NForza.TypedIds;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices.ComTypes;

namespace % Namespace %;

[JsonConverter(typeof(% ItemName %JsonConverter))]
public partial record struct % ItemName %(Guid Value): ITypedId
{
    % Constructor %
    public static % ItemName % Empty => new % ItemName %(% Default %);
    % CastOperators %
    public override string ToString() => Value.ToString();
}