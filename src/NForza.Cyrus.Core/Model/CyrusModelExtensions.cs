using System.Text.Json;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

public static class CyrusModelExtensions
{
    static JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };

    public static string AsJson(this ICyrusModel model)
    {
        return JsonSerializer.Serialize(model, options);
    }

    public static ICyrusModel Combine(this ICyrusModel model, params ICyrusModel[] models)
    {
        return new CyrusModelAggregator([model, .. models]);
    }
}
