using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace NForza.Cyrus.Templating;


public class ResourceTemplateFileInfo : IFileInfo
{
    private readonly Assembly assembly;
    private readonly string resourceName;

    public ResourceTemplateFileInfo(Assembly assembly, string resourceName)
    {
        this.assembly = assembly;
        this.resourceName = resourceName;
    }

    public bool Exists => true;

    public long Length
    {
        get
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                return stream?.Length ?? -1;
            }
        }
    }

    public string PhysicalPath => null;

    public string Name => resourceName;

    public DateTimeOffset LastModified => DateTimeOffset.MinValue;

    public bool IsDirectory => false;

    public Stream CreateReadStream()
    {
        Stream stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Resource {resourceName} not found in assembly {assembly.FullName}.");
        }
        return stream;
    }
}