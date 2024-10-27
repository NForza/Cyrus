using System;
using NForza.TypedIds;
using System.Text.Json.Serialization;

namespace % NamespaceName %
{
    [JsonConverter(typeof(% ItemName %JsonConverter))]
    public partial record struct % ItemName %(string Value): ITypedId
    {
        public static % ItemName % Empty => new % ItemName %(string.Empty);
        public bool IsNullOrEmpty() => string.IsNullOrEmpty(Value);
        % CastOperators %
        % IsValid %
    }
}