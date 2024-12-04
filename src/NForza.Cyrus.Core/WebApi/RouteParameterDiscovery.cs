using System.Text.RegularExpressions;

namespace NForza.Cyrus.WebApi
{
    public static partial class RouteParameterDiscovery
    {
        public static IEnumerable<ParameterDescription> FindAllParametersInRoute(string route)
        {
            var rgx = ParameterRegex();
            MatchCollection matches = rgx.Matches(route);
            return matches.Select(m => new ParameterDescription(m.Groups["parameter"].Value, GetSwaggerType(m.Groups["type"].Value), GetSwaggerFormat(m.Groups["type"].Value)));
        }

        private static string GetSwaggerFormat(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "guid" => "guid",
                _ => ""
            };
        }

        private static string GetSwaggerType(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "int" or "long" => "integer",
                _ => "string"
            };
        }

        [GeneratedRegex(@"\{(?<parameter>[a-zA-Z_][a-zA-Z0-9_]*)(:(?<type>[^}]+))?\}")]
        private static partial Regex ParameterRegex();
    }

    public record struct ParameterDescription(string Name, string Type, string Format)
    {
    }
}