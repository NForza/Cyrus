using System;
using System.Linq;
using System.Reflection;
using System.Text;
using MassTransit;
using MassTransit.RabbitMqTransport.Topology;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.MassTransit;

public static class CyrusModelExtensions
{
    public static string AsAsyncApiYaml(this ICyrusModel model, IBus bus)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (bus is null) throw new ArgumentNullException(nameof(bus));

        var sb = new StringBuilder();

        sb.AppendLine("asyncapi: 2.6.0");
        sb.AppendLine("info:");
        sb.AppendLine("  title: Event Documentation");
        sb.AppendLine("  version: 1.0.0");
        sb.AppendLine();
        sb.AppendLine("channels:");

        foreach (var e in model.Events)
        {
            var messageType = Type.GetType(e.ClrTypeName) ?? throw new InvalidOperationException("Event Type is null");
            var entityName = !string.IsNullOrWhiteSpace(e.Channel)
                ? e.Channel
                : GetEntityNameFromBusTopology(bus, messageType);

            sb.AppendLine($"  {entityName}:");
            sb.AppendLine("    publish:");
            //if (!string.IsNullOrWhiteSpace(e.Summary))
            //    sb.AppendLine($"      summary: {YamlEscapeInline(e.Summary)}");

            sb.AppendLine("      message:");
            sb.AppendLine($"        name: {messageType.Name}");
            sb.AppendLine("        contentType: application/json");
            sb.AppendLine("        payload:");
            sb.AppendLine("          type: object");

            var props = messageType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (props.Length > 0)
            {
                sb.AppendLine("          properties:");
                foreach (var prop in props)
                {
                    var (jsonType, format, itemsType) = MapToJsonType(prop.PropertyType);
                    sb.AppendLine($"            {ToCamelCase(prop.Name)}:");
                    sb.AppendLine($"              type: {jsonType}");
                    if (format is not null)
                        sb.AppendLine($"              format: {format}");
                    if (itemsType is not null)
                    {
                        sb.AppendLine("              items:");
                        sb.AppendLine($"                type: {itemsType}");
                    }
                }
            }
        }

        return sb.ToString();
    }

    // --- helpers ---

    // Reflect: bus.Topology.GetMessageTopology<T>().EntityName
    private static string GetEntityNameFromBusTopology(IBus bus, Type messageType)
    {
        var topology = bus.Topology.PublishTopology.GetMessageTopology(messageType); 
        //RabbitMQ only!
        Exchange exchange = topology.GetType().GetProperty("Exchange")?.GetValue(topology) as Exchange ?? throw new InvalidOperationException("Exchange property is null");

        return string.IsNullOrWhiteSpace(exchange.ExchangeName)
            ? (messageType.FullName ?? messageType.Name)
            : exchange.ExchangeName;
    }

    private static string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s) || char.IsLower(s[0])) return s;
        return char.ToLowerInvariant(s[0]) + s[1..];
    }

    private static (string type, string? format, string? itemsType) MapToJsonType(Type t)
    {
        if (Nullable.GetUnderlyingType(t) is Type u) t = u;

        if (t.IsArray)
            return ("array", null, MapToJsonType(t.GetElementType()!).type);

        var enumOfT = t.GetInterfaces().Concat(new[] { t })
            .FirstOrDefault(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>));
        if (enumOfT != null)
        {
            var elem = enumOfT.GetGenericArguments()[0];
            if (elem != typeof(char))
                return ("array", null, MapToJsonType(elem).type);
        }

        if (t == typeof(string)) return ("string", null, null);
        if (t == typeof(bool)) return ("boolean", null, null);
        if (t == typeof(Guid)) return ("string", "uuid", null);
        if (t == typeof(DateTime) || t == typeof(DateTimeOffset)) return ("string", "date-time", null);
        if (t == typeof(TimeSpan)) return ("string", "duration", null);

        if (t == typeof(byte) || t == typeof(sbyte) ||
            t == typeof(short) || t == typeof(ushort) ||
            t == typeof(int) || t == typeof(uint) ||
            t == typeof(long) || t == typeof(ulong))
            return ("integer", null, null);

        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
            return ("number", null, null);

        if (t.IsEnum) return ("string", null, null);

        return ("object", null, null);
    }
}
