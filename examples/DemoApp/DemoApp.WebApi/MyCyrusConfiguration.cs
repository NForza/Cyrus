using NForza.Cyrus.Abstractions;

namespace DemoApp.WebApi;

public class MyCyrusConfiguration : CyrusConfig
{
    public MyCyrusConfiguration()
    {
        GenerateWebApi();
        UseMassTransit();
        //Note: the DbContext needs to be a fully qualified name including the namespace
        //This is an issue with the Cyrus generator and will (might, actually) be fixed in a future release.
        UseEntityFrameworkPersistence<global::DemoApp.WebApi.DemoDbContext>();
    }
}
