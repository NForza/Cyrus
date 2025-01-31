using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

public static class CyrusModel
{
    public static ICyrusModel Aggregate(IServiceProvider serviceProvider)
    {
        var models = serviceProvider.GetServices<ICyrusModel>();
        return new CyrusModelAggregator(models);
    }
}