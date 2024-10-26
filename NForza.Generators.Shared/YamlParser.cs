using System.Collections.Generic;

namespace NForza.Generators;

public class YamlParser
{
    public static Dictionary<string, List<string>> ReadYaml(string filePath)
    {
        var result = new Dictionary<string, List<string>>();
        string? currentKey = null;

        foreach (var line in filePath.Split('\n'))
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            if (trimmedLine.StartsWith("-"))
            {
                if (currentKey != null && result.ContainsKey(currentKey))
                {
                    result[currentKey].Add(trimmedLine.Substring(1).Trim());
                }
            }
            else
            {
                var parts = trimmedLine.Split([':'], 2);
                if (parts.Length == 2)
                {
                    currentKey = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (!result.ContainsKey(currentKey))
                    {
                        result[currentKey] = [];
                    }
                    result[currentKey].Add(value);
                }
            }
        }

        return result;
    }
}