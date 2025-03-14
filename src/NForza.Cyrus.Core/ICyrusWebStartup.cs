using Microsoft.AspNetCore.Builder;

namespace NForza.Cyrus.WebApi;

public interface ICyrusWebStartup
{
    void AddStartup(WebApplication app);
}