using NForza.Cyrus.Abstractions;

namespace DemoApp.WebApi;

public class CyrusConfiguration : CyrusConfig
{
    public CyrusConfiguration()
    {
        GenerateWebApi();
        UseMassTransit();
    }
}
