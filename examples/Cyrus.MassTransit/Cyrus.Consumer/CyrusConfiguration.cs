using NForza.Cyrus.Abstractions;

namespace Cyrus.Consumer;

public class CyrusConfiguration : CyrusConfig
{
    public CyrusConfiguration()
    {
        UseMassTransit();
        GenerateDomain();
    }
}
