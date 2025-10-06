using System.IO;

namespace NForza.Cyrus.Abstractions;

public class FileResult(Stream stream, string contentType)
{
    public Stream Stream { get; } = stream;
    public string ContentType { get; } = contentType;
}