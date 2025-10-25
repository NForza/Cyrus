using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace NForza.Cyrus.Templating;

internal class TemplateOverrideFileInfo : IFileInfo
{
    private readonly string path;
    private readonly string template;

    public TemplateOverrideFileInfo(string path, string template)
    {
        this.path = path;
        this.template = template;
    }

    public bool Exists => true;

    public long Length => template.Length;

    public string PhysicalPath => "";

    public string Name => path;

    public DateTimeOffset LastModified => DateTimeOffset.MinValue;

    public bool IsDirectory => false;

    public Stream CreateReadStream() => new MemoryStream(Encoding.UTF8.GetBytes(template));
}