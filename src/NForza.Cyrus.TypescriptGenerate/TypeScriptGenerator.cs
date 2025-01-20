using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NForza.Cyrus.TypescriptGenerate.Model;
using Scriban;

namespace NForza.Cyrus.TypescriptGenerate
{
    internal static class TypeScriptGenerator
    {
        public static void Generate(string metadataFile, string outputFolder)
        {
            var json = File.ReadAllText(metadataFile);
            var metadata = JsonSerializer.Deserialize<CyrusMetadata>(json) ?? throw new InvalidOperationException("Can't read metadata");

            GenerateGuids(outputFolder, metadata);
            GenerateStrings(outputFolder, metadata);
            GenerateCommands(outputFolder, metadata);
        }

        private static void GenerateGuids(string outputFolder, CyrusMetadata metadata)
        {
            Template template = GetTemplate("guid");
            foreach (var guid in metadata.Guids)
            {
                var result = template.Render(new { Name = guid });
                string fileName = Path.ChangeExtension(Path.Combine(outputFolder, guid), ".ts");
                File.WriteAllText(fileName, result);
            }
        }

        private static void GenerateStrings(string outputFolder, CyrusMetadata metadata)
        {
            Template template = GetTemplate("string");
            foreach (var stringType in metadata.Strings)
            {
                var result = template.Render(new { Name = stringType });
                string fileName = Path.ChangeExtension(Path.Combine(outputFolder, stringType), ".ts");
                File.WriteAllText(fileName, result);
            }
        }

        private static void GenerateCommands(string outputFolder, CyrusMetadata metadata)
        {
            Template template = GetTemplate("interface");
            if(template.HasErrors)
            {
                throw new InvalidOperationException("Template has errors");
            }
            foreach (var command in metadata.Commands)
            {
                var model = new { Imports = GetImportsFor(metadata, command).ToList(), command.Name, command.Properties };
                var result = template.Render(model);
                string fileName = Path.ChangeExtension(Path.Combine(outputFolder, command.Name), ".ts");
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
}