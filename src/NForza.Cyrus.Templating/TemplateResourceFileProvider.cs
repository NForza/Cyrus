using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace NForza.Cyrus.Templating
{
    public class TemplateResourceFileProvider : IFileProvider
    {
        IEnumerable<string> resourceNames = Enumerable.Empty<string>();
        private Assembly assembly;

        public TemplateResourceFileProvider(Assembly assembly)
        {
            resourceNames = assembly
                .GetManifestResourceNames();
            this.assembly = assembly;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new EnumerableDirectoryContents(resourceNames.Select(x => new EmbeddedResourceFileInfo(assembly, x)));
        }

        public IFileInfo GetFileInfo(string templateName)
        {
            if (!templateName.EndsWith(".liquid"))
            {
                templateName += ".liquid";
            }
            var fullName = resourceNames.SingleOrDefault(rn => rn.EndsWith(templateName)) ?? throw new InvalidOperationException($"Template {templateName} not found.");
            return new EmbeddedResourceFileInfo(assembly, fullName);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }

    public class EnumerableDirectoryContents : IDirectoryContents
    {
        private readonly IEnumerable<IFileInfo> _files;

        public EnumerableDirectoryContents(IEnumerable<IFileInfo> files)
        {
            _files = files;
        }

        public bool Exists => true;

        public IEnumerator<IFileInfo> GetEnumerator() => _files.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _files.GetEnumerator();
    }


    public class EmbeddedResourceFileInfo : IFileInfo
    {
        private readonly Assembly assembly;
        private readonly string resourceName;

        public EmbeddedResourceFileInfo(Assembly assembly, string resourceName)
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
}