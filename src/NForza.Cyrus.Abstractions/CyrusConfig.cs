namespace NForza.Cyrus.Abstractions;

public class CyrusConfig
{
    public CyrusConfig UseMassTransit() => this;
    public CyrusConfig UseEntityFrameworkPersistence<T>() => this;
    public CyrusConfig GenerateDomain() => this;
    public CyrusConfig GenerateContracts() => this;
    public CyrusConfig GenerateWebApi() => this;
}
