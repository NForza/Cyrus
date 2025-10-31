using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Fluid.Values;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Templating;

namespace Cyrus;

internal static class TypeScriptGenerator
{
    private static LiquidEngine? _liquidEngine = null;
    private static LiquidEngine liquidEngine => _liquidEngine ??= new LiquidEngine(Assembly.GetExecutingAssembly(), [], options =>
    {
        options.Filters.AddFilter("to-tstype", (input, arguments, context) =>
        {
            string value = input.ToStringValue();
            return new StringValue(CSharpToTypeScriptType(value, metadata));
        });

        options.Filters.AddFilter("to-tsdefault", (input, arguments, context) =>
        {
            ModelTypeDefinition? value = input.ToObjectValue() as ModelTypeDefinition;
            if (value == null) return input;
            return new StringValue(GetString(value));

            static string GetString(ModelTypeDefinition p)
            {
                if (p.IsNullable) return "null";
                if (p.IsCollection) return "[]";
                var tsType = CSharpToTypeScriptType(p.Name, metadata);
                if (metadata?.Guids.Contains(tsType) ?? false) return "''";
                if (metadata?.Integers.Contains(tsType) ?? false) return "0";
                if (metadata?.Strings.Contains(tsType) ?? false) return "''";
                if (tsType == "string") return "''";
                if (tsType == "number") return "0";
                var model = new { p.Properties };
                return liquidEngine.Render(model, "type-defaults");
            }
        });

        options.Filters.AddFilter("strip-postfix", (input, arguments, context) =>
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

        options.Filters.AddFilter("strip-leading-slash", (input, arguments, context) =>
        {
            string value = input.ToStringValue();

            if (value.StartsWith("/", StringComparison.Ordinal))
                return new StringValue(value.Substring(1));

            return input;
        });
    });

    private static CyrusMetadata? metadata;

    private static string CSharpToTypeScriptType(string input, CyrusMetadata? metadata)
    {
        static bool TypeFoundIn(IEnumerable<ModelTypeDefinition>? models, string input, [NotNullWhen(true)] out string? value)
        {

            var model = models?.FirstOrDefault(m => m.Name == input);
            if (model != null)
            {
                value = model.Name + (model.IsNullable ? "?" : "") + (model.IsCollection ? "[]" : "");
                return true;
            }
            value = null;
            return false;
        }

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
            _ when metadata?.Guids.Contains(input) ?? false => input,
            _ when metadata?.Strings.Contains(input) ?? false => input,
            _ when metadata?.Integers.Contains(input) ?? false => input,
            _ when TypeFoundIn(metadata?.Commands, input, out var value) => value,
            _ when TypeFoundIn(metadata?.Queries, input, out var value) => value,
            _ when TypeFoundIn(metadata?.Events, input, out var value) => value,
            _ when TypeFoundIn(metadata?.Models, input, out var value) => value,
            _ when TypeFoundIn(metadata?.Queries.Select(q => q.ReturnType), input, out var value) => value,
            _ => "any"
        };
        return typeScriptType;
    }

    public static bool Generate(string json, string outputFolder)
    {
        metadata = JsonSerializer.Deserialize<CyrusMetadata>(json, ModelSerializerOptions.Default) ?? throw new InvalidOperationException("Can't read metadata");

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
            IEnumerable<ModelQueryDefinition> queries = hub.Queries.Select(queryName => metadata.Queries.First(q => q.Name == queryName));
            IEnumerable<ModelTypeDefinition> queryTypeDefinitions = queries.Cast<ModelTypeDefinition>();
            IEnumerable<ModelTypeDefinition> commands = hub.Commands.Select(c => metadata.Commands.First(m => m.Name == c));
            IEnumerable<ModelTypeDefinition> events = hub.Events.Select(c => metadata.Events.First(m => m.Name == c));
            IEnumerable<ModelTypeDefinition> queryReturnTypes = queries.Select(q => q.ReturnType);
            IEnumerable<ModelTypeDefinition> allTypes =
                queryTypeDefinitions
                    .Concat(queryReturnTypes)
                    .Concat(commands)
                    .Concat(events)
                    .Distinct(TypeWithPropertiesEqualityComparer.Instance);

            var imports = allTypes.Select(g => g.Name).ToList();

            var result = liquidEngine.Render(new { Imports = imports, Queries = queries, hub.Commands, hub.Events, hub.Path, hub.Name }, "hub");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, hub.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateHubQueryReturnTypes(string outputFolder, CyrusMetadata metadata)
    {
        var returnTypes = metadata.Hubs.SelectMany(h => h.Queries).Select(queryName => metadata.Queries.First(q => q.Name == queryName).ReturnType).Distinct(TypeWithPropertiesEqualityComparer.Instance);
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
        foreach (var type in typesWithProperties)
        {
            var model = new { Imports = GetImportsFor(type, false, metadata).ToList(), type.Name, type.Properties };
            var result = liquidEngine.Render(model, "interface");
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, type.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateQueries(string outputFolder, CyrusMetadata metadata)
    {
        GenerateTypesWithProperties(metadata.Queries, outputFolder, metadata);
        GenerateTypesWithProperties(metadata.Queries.Select(q => q.ReturnType), outputFolder, metadata);
    }

    private static void GenerateIntegers(string outputFolder, CyrusMetadata metadata)
    {
        foreach (var integerType in metadata.Integers)
        {
            var result = liquidEngine.Render(new { Name = integerType }, "number");
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
        IEnumerable<Import> GetAllImports()
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

        return GetAllImports().GroupBy(i => i.Name).Select(g => g.First());
    }
}