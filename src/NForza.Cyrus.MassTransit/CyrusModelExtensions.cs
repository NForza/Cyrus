using System;
using System.Linq;
using System.Reflection;
using System.Text;
using MassTransit;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.MassTransit;

public static class CyrusModelExtensions
{
    public static string AsAsyncApiYaml(this ICyrusModel model, IPublishTopology publishTopology)
    {
        ArgumentNullException.ThrowIfNull(publishTopology);

        //var messageTopology = bus.Topology.Message; // IMessageTopology
        //var events = model.Events;
        var sb = new StringBuilder();

        //sb.AppendLine("asyncapi: 2.6.0");
        //sb.AppendLine("info:");
        //sb.AppendLine("  title: Event Documentation");
        //sb.AppendLine("  version: 1.0.0");
        //sb.AppendLine();
        //sb.AppendLine("channels:");

        //foreach (var e in events)
        //{
        //    var messageType = e.Type ?? throw new InvalidOperationException("Event Type is null");
        //    var msgTopo = messageTopology.GetMessageTopology(messageType);
        //    var entityName = msgTopo?.EntityName ?? messageType.FullName!;
        //    var channel = string.IsNullOrWhiteSpace(e.Channel) ? entityName : e.Channel;

        //    sb.AppendLine($"  {channel}:");
        //    sb.AppendLine("    publish:");
        //    if (!string.IsNullOrWhiteSpace(e.Summary))
        //        sb.AppendLine($"      summary: {Escape(e.Summary)}");

        //    sb.AppendLine("      message:");
        //    sb.AppendLine($"        name: {messageType.Name}");
        //    sb.AppendLine("        contentType: application/json");
        //    sb.AppendLine("        payload:");
        //    sb.AppendLine("          type: object");

        //    var props = messageType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    if (props.Length > 0)
        //    {
        //        sb.AppendLine("          properties:");
        //        foreach (var prop in props)
        //        {
        //            var (jsonType, format, items) = MapToJsonType(prop.PropertyType);
        //            sb.AppendLine($"            {ToCamelCase(prop.Name)}:");
        //            sb.AppendLine($"              type: {jsonType}");
        //            if (format is not null)
        //                sb.AppendLine($"              format: {format}");
        //            if (items is not null)
        //                sb.AppendLine($"              items:\n                type: {items}");
        //        }
        //    }
        //}

        return sb.ToString();
    }

    private static string Escape(string s) =>
        s.Replace("\r", " ").Replace("\n", " ").Replace(":", "\\:");

    private static string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s) || char.IsLower(s[0])) return s;
        return char.ToLowerInvariant(s[0]) + s.Substring(1);
    }

    private static (string type, string? format, string? itemsType) MapToJsonType(Type t)
    {
        // unwrap Nullable<T>
        if (Nullable.GetUnderlyingType(t) is Type u) t = u;

        // arrays / IEnumerable<T>
        if (t.IsArray)
            return ("array", null, MapToJsonType(t.GetElementType()!).type);

        var ienumT = t.GetInterfaces()
            .Concat(new[] { t })
            .FirstOrDefault(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>));
        if (ienumT != null && ienumT.GetGenericArguments()[0] != typeof(char))
        {
            var elem = ienumT.GetGenericArguments()[0];
            return ("array", null, MapToJsonType(elem).type);
        }

        // primitives
        if (t == typeof(string)) return ("string", null, null);
        if (t == typeof(bool)) return ("boolean", null, null);
        if (t == typeof(Guid)) return ("string", "uuid", null);
        if (t == typeof(DateTime) || t == typeof(DateTimeOffset)) return ("string", "date-time", null);
        if (t == typeof(TimeSpan)) return ("string", "duration", null);
        if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int) || t == typeof(uint) || t == typeof(long))
            return ("integer", null, null);
        if (t == typeof(ulong)) return ("integer", "int64", null);
        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
            return ("number", null, null);

        // fallback: nested object
        return ("object", null, null);
    }
}
