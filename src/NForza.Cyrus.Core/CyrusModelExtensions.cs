using System.Text.Json;
using NForza.Cyrus.TypedIds;

namespace NForza.Cyrus;

public static class CyrusModelExtensions
{
    public static string AsJson(this ICyrusModel model)
    {
        return JsonSerializer.Serialize(model);
    }
    public static ICyrusModel Combine(this ICyrusModel model, ICyrusModel model2)
    {
        return new CyrusModelAggregator(model, model2);
    }

}
