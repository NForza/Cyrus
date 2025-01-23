using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NForza.Cyrus.TypescriptGenerate.Model;
using Scriban;
using Scriban.Runtime;

namespace NForza.Cyrus.TypescriptGenerate;

internal static class TypeScriptGenerator
{
    private static TemplateContext GetContext(object model)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import("camel_case", (string input) =>
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        });
        scriptObject.Import(model, null, null);
        var context = new TemplateContext();
        context.PushGlobal(scriptObject);
        return context;
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
            var result = template.Render(GetContext(hub));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, hub.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateGuids(string outputFolder, CyrusMetadata metadata)
    {
        Template template = GetTemplate("guid");
        foreach (var guid in metadata.Guids)
        {
            var result = template.Render(GetContext(new { Name = guid }));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, guid), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static void GenerateStrings(string outputFolder, CyrusMetadata metadata)
    {
        Template template = GetTemplate("string");
        foreach (var stringType in metadata.Strings)
        {
            var result = template.Render(GetContext(new { Name = stringType }));
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
            var result = template.Render(GetContext(model));
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
            var result = template.Render(GetContext(model));
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
            var result = template.Render(GetContext(model));
            string fileName = Path.ChangeExtension(Path.Combine(outputFolder, @event.Name), ".ts");
            File.WriteAllText(fileName, result);
        }
    }

    private static IEnumerable<Import> GetImportsFor(CyrusMetadata metadata, IMetaDataWithProperties obj)
    {
        foreach(var p in obj.Properties)
        {
            if(metadata.Guids.Contains(p.Type))
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