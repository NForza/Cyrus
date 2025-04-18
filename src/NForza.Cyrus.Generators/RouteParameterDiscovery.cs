using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NForza.Cyrus.Generators;

public static partial class RouteParameterDiscovery
{
    public static IEnumerable<string> FindAllParametersInRoute(string? route)
    {
        if (route == null)
            return [];
        var rgx = ParameterRegex();
        MatchCollection matches = rgx.Matches(route);
        return matches.Cast<Match>().Select(m => m.Groups["parameter"].Value);
    }

    private static readonly Regex ParameterRegexInstance = new Regex(@"\{(?<parameter>[a-zA-Z_][a-zA-Z0-9_]*)(:(?<type>[^}]+))?\}", RegexOptions.Compiled);

    public static Regex ParameterRegex() => ParameterRegexInstance;
}