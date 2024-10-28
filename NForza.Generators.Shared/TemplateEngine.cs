using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NForza.Generators;

public class TemplateEngine(Assembly assembly, string folderName)
{
    public string ReplaceInResourceTemplate(string templateName, Dictionary<string, string> replacements)
    {
        string template = EmbeddedResourceReader.GetResource(assembly, folderName, templateName);
        foreach (var replacement in replacements)
        {
            template = template.Replace( "% " + replacement.Key + " %", replacement.Value);
        }
        ThrowIfTemplateHasUnresolvedMarkers(template);
        return template;
    }

    private void ThrowIfTemplateHasUnresolvedMarkers(string template)
    {
        Regex regex = new Regex("% [a-zA-Z0-9_]{1,30} %");
        MatchCollection matches = regex.Matches(template);
        if (matches.Count > 0)
        {
            throw new InvalidOperationException($"The template has unresolved markers: {string.Join(", ", matches.Cast<Match>().Select(m => m.Value).ToList())}");
        }
    }
}
