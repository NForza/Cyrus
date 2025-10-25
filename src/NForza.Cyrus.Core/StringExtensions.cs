using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace NForza.Cyrus;

public static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }
        str = char.ToLowerInvariant(str[0]) + str.Substring(1);
        return str;
    }

    public static string CompressToBase64(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        byte[] raw = Encoding.UTF8.GetBytes(input);

        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(raw, 0, raw.Length);
        }

        byte[] compressed = ms.ToArray();
        return Convert.ToBase64String(compressed);
    }

    public static string DecompressFromBase64(this string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return base64;

        byte[] compressed = Convert.FromBase64String(base64);

        using var inputMs = new MemoryStream(compressed);
        using var gzip = new GZipStream(inputMs, CompressionMode.Decompress);
        using var outputMs = new MemoryStream();
        gzip.CopyTo(outputMs);

        return Encoding.UTF8.GetString(outputMs.ToArray());
    }
}