using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace NForza.Cyrus.Templating;

public class TemplateProvider : IFileProvider
{
    IEnumerable<string> resourceNames = Enumerable.Empty<string>();
    private Assembly assembly;
    private Dictionary<string, string> templateOverrides;

    public TemplateProvider(Assembly assembly, Dictionary<string, string> templateOverrides)
    {
        resourceNames = assembly
            .GetManifestResourceNames();
        this.assembly = assembly;
        this.templateOverrides = templateOverrides;
    }


    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return new VirtualTemplateFolderContents(
            resourceNames.Select(x => new ResourceFileInfo(assembly, x))
                .Cast<IFileInfo>()
                .Concat(
                    templateOverrides
                        .Select(x => new TemplateOverrideFileInfo(x.Key, x.Value))
                        .Cast<IFileInfo>())
                );
    }

    public IFileInfo GetFileInfo(string templateName)
    {
        if (!templateName.EndsWith(".liquid"))
        {
            templateName += ".liquid";
        }
        var fullName = resourceNames.SingleOrDefault(rn => rn.EndsWith(templateName)) ?? throw new InvalidOperationException($"Template {templateName} not found.");
        return new ResourceFileInfo(assembly, fullName);
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }
}

public class VirtualTemplateFolderContents : IDirectoryContents
{
    private readonly IEnumerable<IFileInfo> _files;

    public VirtualTemplateFolderContents(IEnumerable<IFileInfo> files)
    {
        _files = files;
    }

    public bool Exists => true;

    public IEnumerator<IFileInfo> GetEnumerator() => _files.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _files.GetEnumerator();
}


public class ResourceFileInfo : IFileInfo
{
    private readonly Assembly assembly;
    private readonly string resourceName;

    public ResourceFileInfo(Assembly assembly, string resourceName)
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