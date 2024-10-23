using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NForza.TypedIds.Generator
{
    internal class EmbeddedResourceReader
    {
        internal static string GetResource(string folderName, string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = Path.Combine("NForza.TypedIds.Generator", folderName, fileName).Replace("\\", ".");
            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var names = assembly.GetManifestResourceNames();
                throw new InvalidOperationException($"{resourceName} not found. ResourceNames: {string.Join(",", names)}");
            }

            using StreamReader reader = new(stream);
            string content = reader.ReadToEnd();
            return content;
        }
    }
}