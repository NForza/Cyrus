using NForza.Cyrus.Abstractions;

namespace SimpleCyrusWebApi;

public class CyrusConfiguration : CyrusConfig
{
    public CyrusConfiguration()
    {
        GenerateDomain();
    }
}
