﻿namespace NForza.Cyrus.Abstractions
{
    public class CyrusConfig
    {
        public CyrusConfig CommandSuffix(string commandSuffix) => this;
        public CyrusConfig CommandHandlerName(string commandHandlerName) => this;
        public CyrusConfig UseMassTransit() => this;
        public CyrusConfig EventSuffix(string eventSuffix) => this;
        public CyrusConfig EventHandlerName(string eventHandlerName) => this;
        public CyrusConfig QuerySuffix(string querySuffix) => this;
        public CyrusConfig QueryHandlerName(string queryHandlerName) => this;
        public CyrusConfig GenerateDomain() => this;
        public CyrusConfig UseContractsFromAssembliesContaining(params string[] contracts) => this;
        public CyrusConfig GenerateContracts() => this;
        public CyrusConfig GenerateWebApi() => this;
    }
}
