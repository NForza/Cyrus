using NForza.Cyrus;

namespace SimpleCyrusWebApi
{
    public class CyrusConfiguration : CyrusConfig
    {
        public CyrusConfiguration()
        {
            UseMassTransit();
        }
    }
}
