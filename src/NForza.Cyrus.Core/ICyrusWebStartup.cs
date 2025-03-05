using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace NForza.Cyrus.WebApi;

public interface ICyrusWebStartup
{
    void AddStartup(WebApplication app);
}