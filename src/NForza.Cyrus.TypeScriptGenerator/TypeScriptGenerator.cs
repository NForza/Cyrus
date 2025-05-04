using System.Reflection;
using System.Text.Json;
using Cyrus.Model;
using Fluid.Values;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Templating;

namespace Cyrus;

internal static class TypeScriptGenerator
{
    private static LiquidEngine? _liquidEngine = null;
    private static LiquidEngine liquidEngine => _liquidEngine ??= new LiquidEngine(Assembly.GetExecutingAssembly(), options =>
    {
        options.Filters.AddFilter("to_typescript_type", (input, arguments, context) =>
        {
            string value = input.ToStringValue();
            return new StringValue(CSharpToTypeScriptType(value, metadata));
        });

        options.Filters.AddFilter("to_typescript_default", (input, arguments, context) =>
        {
            ModelTypeDefinition value = input.ToObjectValue() as ModelTypeDefinition;
            return new StringValue(GetString(value));

            static string GetString(ModelTypeDefinition p)
            {
                if (p.IsNullable) return "null";
                if (p.IsCollection) return "[]";
                var tsType = CSharpToTypeScriptType(p.Name, metadata);
                if (metadata.Guids.Contains(tsType)) return "''";
                if (metadata.Integers.Contains(tsType)) return "0";
                if (metadata.Strings.Contains(tsType)) return "''";
                if (tsType == "string") return "''";
                if (tsType == "number") return "0";
                var model = new { p.Properties };
                return liquidEngine.Render(model, "type-defaults");
            }
        });

        options.Filters.AddFilter("strip_postfix", (input, arguments, context) =>
        {
            string value = input.ToStringValue();
            const string commandSuffix = "Command";
            const string querySuffix = "Query";
            const string eventSuffix = "Event";

            if (value.EndsWith(commandSuffix, StringComparison.Ordinal))
                return new StringValue(value.Substring(0, value.Length - commandSuffix.Length));

            if (value.EndsWith(querySuffix, StringComparison.Ordinal))
                return new StringValue(value.Substring(0, value.Length - querySuffix.Length));

            if (value.EndsWith(eventSuffix, StringComparison.Ordinal))
                return new StringValue(value.Substring(0, value.Length - eventSuffix.Length));

            return input;
        });

        options.Filters.AddFilter("query_return_type", (input, arguments, context) =>
        {
            ModelQueryDefinition value = input.ToObjectValue() as ModelQueryDefinition;
            var tsType = CSharpToTypeScriptType(value.ReturnType.Name, metadata);
            return new StringValue(value.ReturnType.Name + (value.ReturnType.IsNullable ? "?" : "") + (value.ReturnType.IsCollection ? "[]" : ""));
        });
    });

    private static CyrusMetadata metadata;

    private static string CSharpToTypeScriptType(string input, CyrusMetadata metadata)
    {
        if (string.IsNullOrEmpty(input)) return "";

        var typeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "string", "string" },
            { "int", "number" },
            { "long", "number" },
            { "short", "number" },
            { "byte", "number" },
            { "float", "number" },
            { "double", "number" },
            { "decimal", "number" },
            { "bool", "boolean" },
            { "char", "string" },
            { "object", "any" },
            { "void", "void" },
            { "DateTime", "Date" },
            { "Guid", "string" }
        };

        string typeScriptType = input switch
        {
            _ when typeMapping.TryGetValue(input, out string? tsType) => tsType,
            _ when metadata.Guids.Contains(input) => input,
            _ when metadata.Strings.Contains(input) => input,
            _ when metadata.Integers.Contains(input) => input,
            _ when metadata.Commands.Any(c => c.Name == input) => input,
            _ when metadata.Queries.Any(c => c.Name == input) => input,
            _ when metadata.Events.Any(c => c.Name == input) => input,
            _ when metadata.Models.Any(c => c.Name == input) => input,
            _ => "any"
        };
        return typeScriptType;
    }

    public static bool Generate(string json, string outputFolder)
    {
        metadata = JsonSerializer.Deserialize<CyrusMetadata>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new InvalidOperationException("Can't read metadata");

        var path = Path.GetFullPath(outputFolder);
        Console.WriteLine("model: " + JsonSerializer.Serialize(metadata));

        Console.WriteLine("Writing output to: " + path);

        Console.WriteLine("Writing Guids.");
        GenerateGuids(path, metadata);

        Console.WriteLine("Writing Strings.");
        GenerateStrings(path, metadata);

        Console.WriteLine("Writing Integers.");
        GenerateIntegers(path, metadata);

        Console.WriteLine("Writing Commands.");
        GenerateCommands(path, metadata);

        Console.WriteLine("Writing Queries.");
        GenerateQueries(path, metadata);

        Console.WriteLine("Writing Events.");
        GenerateEvents(path, metadata);

        Console.WriteLine("Writing Hubs.");
        GenerateHubs(path, metadata);

        Console.WriteLine("Writing Models.");
        GenerateModels(path, metadata);
        return true;
    }

    private static void GenerateModels(string outputFolder, CyrusMetadata metadata)
    {
        foreach (var type in metadata.Models)
        {
            string result = "";
            bool hasValues = type.Values?.Any() ?? false;
            if (hasValues)
            {
                result = liquidEngine.Render(type, "enum");
            }
            else
            {
                var model = new { Imports = GetImportsFor(type, false, metadata).ToList(), type.Name, type.Properties };
                result = liquidEngine.Render(model, "interface");
            }
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, type.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateHubs(string outputFolder, CyrusMetadata metadata)
    {
        GenerateHubQueryReturnTypes(outputFolder, metadata);
        foreach (var hub in metadata.Hubs)
        {
            var queries = hub.Queries.Select(q => metadata.Queries.First(m => m.Name == q.Name));
            var commands = hub.Commands.Select(c => metadata.Commands.First(m => m.Name == c));
            var events = hub.Events.Select(c => metadata.Events.First(m => m.Name == c));
            var queryReturnTypes = hub.Queries.Select(q => q.ReturnType);
            var allTypes = queries.Concat(commands).Concat(queryReturnTypes).Concat(events).Distinct(TypeWithPropertiesEqualityComparer.Instance);

            var imports = allTypes.Select(g => g.Name).ToList();

            var result = liquidEngine.Render(new { Imports = imports, hub.Queries, hub.Commands, hub.Events, hub.Path, hub.Name }, "hub");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, hub.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateHubQueryReturnTypes(string outputFolder, CyrusMetadata metadata)
    {
        var returnTypes = metadata.Hubs.SelectMany(h => h.Queries).Select(q => q.ReturnType).Distinct(TypeWithPropertiesEqualityComparer.Instance);
        GenerateTypesWithProperties(returnTypes, outputFolder, metadata);
    }

    private static void GenerateGuids(string outputFolder, CyrusMetadata metadata)
    {
        foreach (var guid in metadata.Guids)
        {
            var result = liquidEngine.Render(new { Name = guid }, "guid");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, guid), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateStrings(string outputFolder, CyrusMetadata metadata)
    {
        foreach (var stringType in metadata.Strings)
        {
            var result = liquidEngine.Render(new { Name = stringType }, "string");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, stringType), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateCommands(string outputFolder, CyrusMetadata metadata) => GenerateTypesWithProperties(metadata.Commands, outputFolder, metadata);

    private static void GenerateTypesWithProperties(IEnumerable<ModelTypeDefinition> typesWithProperties, string outputFolder, CyrusMetadata metadata)
    {
        foreach (var command in typesWithProperties)
        {
            var model = new { Imports = GetImportsFor(command, false, metadata).ToList(), command.Name, command.Properties };
            var result = liquidEngine.Render(model, "interface");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, command.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateQueries(string outputFolder, CyrusMetadata metadata) => GenerateTypesWithProperties(metadata.Queries, outputFolder, metadata);
    private static void GenerateIntegers(string outputFolder, CyrusMetadata metadata)
    {
        foreach (var integerType in metadata.Integers)
        {
            var result = liquidEngine.Render(new { Name = integerType }, "integer");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, integerType), ".ts");
            File.WriteAllText(fileName, result);
        }
    }
    private static void GenerateEvents(string outputFolder, CyrusMetadata metadata)
    {
        foreach (var @event in metadata.Events)
        {
            var model = new { Imports = GetImportsFor(@event, false, metadata).ToList(), @event.Name, @event.Properties };
            var result = liquidEngine.Render(model, "interface");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, @event.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static IEnumerable<Import> GetImportsFor(ModelTypeDefinition type, bool includeTypeItself, CyrusMetadata metadata)
    {
        if (includeTypeItself)
        {
            yield return new Import { Name = type.Name };
        }
        foreach (var p in type.Properties ?? [])
        {
            if (metadata.Guids.Contains(p.Type))
            {
                yield return new Import { Name = p.Type };
            }
            if (metadata.Strings.Contains(p.Type))
            {
                yield return new Import { Name = p.Type };
            }
            if (metadata.Integers.Contains(p.Type))
            {
                yield return new Import { Name = p.Type };
            }
            if (metadata.Models.Any(m => m.Name == p.Type))
            {
                yield return new Import { Name = p.Type };
            }
        }
    }
}