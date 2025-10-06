using System.IO;

namespace NForza.Cyrus;

public record FileResult(Stream Stream, string ContentType);