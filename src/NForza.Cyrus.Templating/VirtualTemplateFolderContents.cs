using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace NForza.Cyrus.Templating;

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
