using System.IO;
using Microsoft.Extensions.FileProviders;

namespace NForza.Cyrus.Templating;

public static class FileProviderExtensions
{
    public static string GetTemplateContents(this IFileProvider provider, string path)
      => provider.GetFileContents(path);

    public static string GetFileContents(this IFileProvider provider, string path)
    {
        var file = provider.GetFileInfo(path);
        using var reader = new StreamReader(file.CreateReadStream());
        string contents = reader.ReadToEnd();
        return contents;
    }
}
