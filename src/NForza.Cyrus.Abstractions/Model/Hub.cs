using System;

namespace NForza.Cyrus.Abstractions.Model;

public class Hub
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string[] Events { get; set; } = Array.Empty<string>();
    public Query[] Queries { get; set; } = Array.Empty<Query>();
    public string[] Commands { get; set; } = Array.Empty<string>();
}