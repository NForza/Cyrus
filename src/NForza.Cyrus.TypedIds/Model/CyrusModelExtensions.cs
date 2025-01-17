using System.Text.Json;
using NForza.Cyrus.TypedIds;

namespace NForza.Cyrus.TypedIds.Model;

public static class CyrusModelExtensions
{
    public static string AsJson(this ICyrusModel model)
    {
        return JsonSerializer.Serialize(model);
    }
    public static ICyrusModel Combine(this ICyrusModel model, params ICyrusModel[] models)
    {
        return new CyrusModelAggregator([model, ..models]);
    }

}
