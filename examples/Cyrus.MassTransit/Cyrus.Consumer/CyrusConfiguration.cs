using NForza.Cyrus;

namespace Cyrus.Server
{
    public class CyrusConfiguration : CyrusConfig
    {
        public CyrusConfiguration()
        {
            UseMassTransit();
            GenerateWebApi();
            GenerateDomain();
            UseContractsFromAssembliesContaining("Messages");
        }
    }
}
