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

        scriptObject.Import(model, null, null);
        context.PushGlobal(scriptObject);
        context.SetValue(metaDataVariable, metadata);
        return context;
    }

    private static string CSharpToTypeScriptType(string input, CyrusMetadata metadata)
    {
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
        Template template = GetTemplate("hub");
        foreach (var hub in metadata.Hubs)
        {
            hub.Imports = hub.Events.Concat(hub.Commands).Concat(hub.Queries).ToArray();
            var result = template.Render(GetContext(hub, metadata));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, hub.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
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

    private static void GenerateCommands(string outputFolder, CyrusMetadata metadata)
    {
        Template template = GetTemplate("interface");
        foreach (var command in metadata.Commands)
        {
            var model = new { Imports = GetImportsFor(metadata, command).ToList(), command.Name, command.Properties };
            var result = template.Render(GetContext(model, metadata));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, command.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateQueries(string outputFolder, CyrusMetadata metadata)
    {
        Template template = GetTemplate("interface");
        foreach (var query in metadata.Queries)
        {
            var model = new { Imports = GetImportsFor(metadata, query).ToList(), query.Name, query.Properties };
            var result = template.Render(GetContext(model, metadata));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, query.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

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

    private static IEnumerable<Import> GetImportsFor(CyrusMetadata metadata, IMetaDataWithProperties obj)
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
        var template = Template.Parse(templateContent);
        return template;
    }
}