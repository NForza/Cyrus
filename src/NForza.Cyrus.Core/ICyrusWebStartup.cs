using Microsoft.AspNetCore.Builder;

namespace NForza.Cyrus;

public interface ICyrusWebStartup
{
    void AddStartup(WebApplication app);
}