using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace NForza.Cyrus.Templating;

public class TemplateFileProvider : IFileProvider
{
    IEnumerable<string> resourceNames = Enumerable.Empty<string>();
    private Assembly assembly;
    private Dictionary<string, string> templateOverrides;

    public TemplateFileProvider(Assembly assembly, Dictionary<string, string> templateOverrides)
    {
        resourceNames = assembly
            .GetManifestResourceNames();
        this.assembly = assembly;
        this.templateOverrides = templateOverrides;
    }


    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return new VirtualTemplateFolderContents(
            resourceNames.Select(x => new ResourceTemplateFileInfo(assembly, x))
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

        var hasOverride = templateOverrides.ContainsKey(templateName);
        if (hasOverride)
        {
            string template = templateOverrides[templateName];
            return new TemplateOverrideFileInfo(templateName, template);
        }

        var fullName = resourceNames.SingleOrDefault(rn => rn.EndsWith(templateName)) ?? throw new InvalidOperationException($"Template {templateName} not found.");
        return new ResourceTemplateFileInfo(assembly, fullName);
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }
}
