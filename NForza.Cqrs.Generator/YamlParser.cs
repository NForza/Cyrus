using System.Collections.Generic;

namespace NForza.Cqrs.Generator;

internal class YamlParser
{
    internal static Dictionary<string, List<string>> ReadYaml(string filePath)
    {
        var result = new Dictionary<string, List<string>>();
        string currentKey = null;

        foreach (var line in filePath.Split('\n'))
        {
            var trimmedLine = line.Trim();

            // Skip empty lines or comments
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            if (trimmedLine.StartsWith("-"))
            {
                // Handle list items
                if (currentKey != null && result.ContainsKey(currentKey))
                {
                    result[currentKey].Add(trimmedLine.Substring(1).Trim());
                }
            }
            else
            {
                // Split key and value
                var parts = trimmedLine.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    currentKey = parts[0].Trim();
                    var value = parts[1].Trim();

                    // Initialize the list for this key
                    if (!result.ContainsKey(currentKey))
                    {
                        result[currentKey] = new List<string>();
                    }
                    result[currentKey].Add(value);
                }
            }
        }

        return result;
    }
}

