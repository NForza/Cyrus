using System.Text.Json;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

public static class CyrusModelExtensions
{
    public static string AsJson(this ICyrusModel model)
    {
        return JsonSerializer.Serialize(model, ModelSerializerOptions.Default);
    }

    public static ICyrusModel Combine(this ICyrusModel model, params ICyrusModel[] models)
    {
        return new CyrusModelAggregator([model, .. models]);
    }
}
