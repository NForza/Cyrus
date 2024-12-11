using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class CyrusOptions(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
