using NForza.Cyrus.Abstractions;

namespace Cyrus.Producer;

public class CyrusConfiguration : CyrusConfig
{
    public CyrusConfiguration()
    {
        UseMassTransit();
    }
}
