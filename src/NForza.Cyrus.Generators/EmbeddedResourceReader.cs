﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NForza.Cyrus.Generators;

public class EmbeddedResourceReader
{
    public static string GetResource(Assembly assembly, string folderName, string fileName)
    {
        var names = assembly.GetManifestResourceNames();
        string resourceName = $"{folderName}.{fileName}";
        resourceName = names.FirstOrDefault(n => n.EndsWith(resourceName));
        if (resourceName == null)
        {
            throw new InvalidOperationException($"{resourceName} not found. ResourceNames: {string.Join(",", names)}");
        }
        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        return content;
    }
}