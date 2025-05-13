using NForza.Cyrus.Abstractions;

namespace CyrusDemo;

public class MyCyrusConfiguration : CyrusConfig
{
    public MyCyrusConfiguration()
    {
        UseMassTransit();
    }
}
