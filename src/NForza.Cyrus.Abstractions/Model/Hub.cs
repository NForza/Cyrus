using System;

namespace NForza.Cyrus.Abstractions.Model
{
    public class Hub
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string[] Events { get; set; } = Array.Empty<string>();
        public HubQuery[] Queries { get; set; } = Array.Empty<HubQuery>();
        public string[] Commands { get; set; } = Array.Empty<string>();
    }
}