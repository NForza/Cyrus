using System.IO;

namespace NForza.Cyrus.Abstractions;

public class StreamResult(Stream stream)
{
    public Stream Stream { get; } = stream;
}