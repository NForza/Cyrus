namespace NForza.Cyrus.Abstractions
{
    public class CyrusConfig
    {
        public CyrusConfig UseMassTransit() => this;
        public CyrusConfig QuerySuffix(string querySuffix) => this;
        public CyrusConfig GenerateDomain() => this;
        public CyrusConfig UseContractsFromAssembliesContaining(params string[] contracts) => this;
        public CyrusConfig GenerateContracts() => this;
        public CyrusConfig GenerateWebApi() => this;
    }
}
