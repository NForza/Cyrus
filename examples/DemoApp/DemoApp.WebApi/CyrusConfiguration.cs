using NForza.Cyrus.TypedIds;

namespace DemoApp.WebApi;

public class CyrusConfiguration : CyrusConfig
{
    public CyrusConfiguration()
    {
        GenerateWebApi();
        UseContractsFromAssembliesContaining("Contracts");
    }
}
