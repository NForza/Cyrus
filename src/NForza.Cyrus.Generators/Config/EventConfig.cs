﻿namespace NForza.Cyrus.Generators.Config
{
    public class EventConfig
    {
        public string Bus { get; set; } = "Local";
        public string Suffix { get; set; } = "Event";
        public string HandlerName { get; set; } = "Handle";
    }
}