using NForza.Cyrus.Abstractions;

namespace DemoApp.WebApi;

public class MyCyrusConfiguration : CyrusConfig
{
    public MyCyrusConfiguration()
    {
        GenerateWebApi();
        UseMassTransit();
        UseEntityFrameworkPersistence<global::DemoApp.WebApi.DemoDbContext>();
    }
}
