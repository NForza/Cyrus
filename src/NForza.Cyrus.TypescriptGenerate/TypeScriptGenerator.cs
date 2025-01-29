using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NForza.Cyrus.TypescriptGenerate.Model;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace NForza.Cyrus.TypescriptGenerate;

internal static class TypeScriptGenerator
{
    private static TemplateContext GetContext(object model, CyrusMetadata metadata)
    {
        var context = new TemplateContext();
        ScriptVariable metaDataVariable = ScriptVariable.Create("metadata", ScriptVariableScope.Global);
        var scriptObject = new ScriptObject();
        scriptObject.Import("camel_case", (string input) =>
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        });

        scriptObject.Import("to_typescript_type", (string input) =>
        {
            var metaData = context.GetValue(metaDataVariable);
            return CSharpToTypeScriptType(input, (CyrusMetadata)metaData);
        });

        scriptObject.Import("to_typescript_default", (Func<ITypeDefinition, string>)(p =>
        {
            if (p.IsNullable) return "null";
            if (p.IsCollection) return "[]";
            var tsType = CSharpToTypeScriptType(p.Type, metadata);
            if (metadata.Guids.Contains(tsType)) return "''";
            if (metadata.Integers.Contains(tsType)) return "0";
            if (metadata.Strings.Contains(tsType)) return "''";
            if (tsType == "string") return "''";
            if (tsType == "number") return "0";
            if (p is ITypeWithProperties twp)
            {
                Template defaults = GetTemplate("type-defaults");
                var model = new { twp.Properties };
                return defaults.Render(GetContext(model, metadata));
            }
            return "{}";
        }));

        scriptObject.Import("strip_postfix", (string input) =>
        {
            if (input.EndsWith("Command")) return input[..^"Command".Length];
            if (input.EndsWith("Query")) return input[..^"Query".Length];
            if (input.EndsWith("Event")) return input[..^"Event".Length];
            return input;
        });

        scriptObject.Import("query_return_type", (Func<HubQuery, string>)(rt =>
        {
            var metaData = context.GetValue(metaDataVariable);
            var tsType = CSharpToTypeScriptType(rt.ReturnType.Name, metadata);
            return rt.ReturnType.Name + (rt.ReturnType.IsNullable ? "?" : "") + (rt.ReturnType.IsCollection ? "[]" : "");
        }));

        scriptObject.Import(model, null, member => member?.Name ?? "");
        context.MemberRenamer = member => member?.Name ?? "";
        context.PushGlobal(scriptObject);
        context.SetValue(metaDataVariable, metadata);
        return context;
    }

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
            _ => "any"
        };
        return typeScriptType;
    }

    public static void Generate(string metadataFile, string outputFolder)
    {
        var json = File.ReadAllText(metadataFile);
        var metadata = JsonSerializer.Deserialize<CyrusMetadata>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new InvalidOperationException("Can't read metadata");

        GenerateGuids(outputFolder, metadata);
        GenerateStrings(outputFolder, metadata);
        GenerateCommands(outputFolder, metadata);
        GenerateQueries(outputFolder, metadata);
        GenerateEvents(outputFolder, metadata);
        GenerateHubs(outputFolder, metadata);
    }

    private static void GenerateHubs(string outputFolder, CyrusMetadata metadata)
    {
        GenerateHubQueryReturnTypes(outputFolder, metadata);
        Template template = GetTemplate("hub");
        foreach (var hub in metadata.Hubs)
        {
            var result = template.Render(GetContext(hub, metadata));
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
        Template template = GetTemplate("guid");
        foreach (var guid in metadata.Guids)
        {
            var result = template.Render(GetContext(new { Name = guid }, metadata));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, guid), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateStrings(string outputFolder, CyrusMetadata metadata)
    {
        Template template = GetTemplate("string");
        foreach (var stringType in metadata.Strings)
        {
            var result = template.Render(GetContext(new { Name = stringType }, metadata));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, stringType), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateCommands(string outputFolder, CyrusMetadata metadata) => GenerateTypesWithProperties(metadata.Commands, outputFolder, metadata);

    private static void GenerateTypesWithProperties(IEnumerable<ITypeWithProperties> typesWithProperties, string outputFolder, CyrusMetadata metadata)
    {
        Template template = GetTemplate("interface");
        foreach (var command in typesWithProperties)
        {
            var model = new { Imports = GetImportsFor(metadata, command).ToList(), command.Name, command.Properties };
            var result = template.Render(GetContext(model, metadata));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, command.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateQueries(string outputFolder, CyrusMetadata metadata) => GenerateTypesWithProperties(metadata.Queries, outputFolder, metadata);

    private static void GenerateEvents(string outputFolder, CyrusMetadata metadata)
    {
        Template template = GetTemplate("interface");
        foreach (var @event in metadata.Events)
        {
            var model = new { Imports = GetImportsFor(metadata, @event).ToList(), @event.Name, @event.Properties };
            var result = template.Render(GetContext(model, metadata));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, @event.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static IEnumerable<Import> GetImportsFor(CyrusMetadata metadata, ITypeWithProperties obj)
    {
        foreach (var p in obj.Properties)
        {
            if (metadata.Guids.Contains(p.Type))
            {
                yield return new Import { Name = p.Type };
            }
            if (metadata.Strings.Contains(p.Type))
            {
                yield return new Import { Name = p.Type };
            }
        }
    }

    private static Template GetTemplate(string templateName)
    {
        var templateContent = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"NForza.Cyrus.TypescriptGenerate.Templates.{templateName}.sbn")!).ReadToEnd();
        var template = Template.Parse(templateContent.Trim());
        return template;
    }
}