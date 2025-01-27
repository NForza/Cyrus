using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;

namespace NForza.Cyrus.Generators
{
    public class ScribanEngine
    {
        static Dictionary<string, Template> templates = [];
        static ScribanEngine()
        {
            LoadResourceTemplates();

        }

        public static string Render(string templateName, object model)
        {
            var template = templates[templateName];
            return template.Render(model, info => info.Name);
        }

        private static void LoadResourceTemplates()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            assembly
                .GetManifestResourceNames()
                .Where(name => name.EndsWith(".sbn"))
                .ToList()
                .ForEach(name =>
                {
                    string[] parts = name.Split('.');
                    string templateName = parts.Take(parts.Length - 1).Last().Replace(".sbn", "");
                    using Stream stream = assembly.GetManifestResourceStream(name);
                    using StreamReader reader = new(stream);
                    string content = reader.ReadToEnd();
                    templates[templateName] = Template.Parse(content);
                });
        }

        public Template GetTemplate(string templateName)
        {
            return templates[templateName];
        }
    }
}
