using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MassTransit;
using MassTransit.RabbitMqTransport.Topology;
using NForza.Cyrus.Abstractions.Model;

public static class AsyncApiWriter
{
    public static string AsAsyncApiYaml(this ICyrusModel model, IBus bus)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (bus is null) throw new ArgumentNullException(nameof(bus));

        var sb = new StringBuilder();
        var indent = new Indent();

        var eventInfos = model.Events
            .Select(e =>
            {
                var messageType = Type.GetType(e.ClrTypeName)
                                  ?? throw new InvalidOperationException($"Event Type is null for '{e.ClrTypeName}'.");
                var entityName = !string.IsNullOrWhiteSpace(e.Channel)
                    ? e.Channel!
                    : GetEntityNameFromBusTopology(bus, messageType);

                return new
                {
                    e.Description,
                    EntityName = entityName,
                    MessageType = messageType,
                    MessageName = messageType.Name
                };
            })
            .ToList();

        var registry = new LocalSchemaRegistry();
        foreach (var ev in eventInfos)
            registry.EnsureSchema(ev.MessageType);

        sb.AppendLine("asyncapi: 2.6.0");
        sb.AppendLine("info:");
        sb.AppendLine("  title: Event Documentation");
        sb.AppendLine("  version: 1.0.0");
        sb.AppendLine();

        sb.AppendLine("channels:");
        foreach (var ev in eventInfos.OrderBy(x => x.EntityName, StringComparer.Ordinal))
        {
            indent.Level = 1;
            sb.AppendLine($"{indent}{YamlKey(ev.EntityName)}:");
            indent.Level = 2;
            sb.AppendLine($"{indent}publish:");
            if (!string.IsNullOrWhiteSpace(ev.Description))
                sb.AppendLine($"{indent}  summary: {YamlEscapeInline(ev.Description!)}");
            sb.AppendLine($"{indent}  message:");
            sb.AppendLine($"{indent}    $ref: '#/components/messages/{ev.MessageName}'");
        }

        sb.AppendLine();
        sb.AppendLine("components:");

        sb.AppendLine("  messages:");
        indent.Level = 2;
        foreach (var ev in eventInfos.OrderBy(x => x.MessageName, StringComparer.Ordinal))
        {
            sb.AppendLine($"{indent}{ev.MessageName}:");
            sb.AppendLine($"{indent}  name: {ev.MessageName}");
            sb.AppendLine($"{indent}  contentType: application/json");
            sb.AppendLine($"{indent}  payload:");
            sb.AppendLine($"{indent}    $ref: '#/components/schemas/{registry.GetSchemaName(ev.MessageType)}'");
        }

        sb.AppendLine("  schemas:");
        indent.Level = 2;
        foreach (var schema in registry.EmitSchemasInDependencyOrder())
        {
            sb.Append(schema.ToYaml(indent));
        }

        return sb.ToString();
    }


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
        if (s.Length == 1) return s.ToLowerInvariant();
        return char.ToLowerInvariant(s[0]) + s[1..];
    }

    private static string YamlEscapeInline(string value)
    {
        var v = value.Replace("\"", "\"\"");
        var needsQuotes = v.Any(ch => char.IsControl(ch) || ":{}[],-&*#?|\t".Contains(ch) || v.Contains('\n'));
        if (v.Contains('\n'))
        {
            return "|\n" + string.Join("\n", v.Split('\n').Select(l => "        " + l));
        }
        return needsQuotes ? $"\"{v}\"" : v;
    }

    private static string YamlKey(string raw) =>
        raw.Any(ch => ch == '.' || ch == '/' || ch == ':' || ch == ' ')
            ? $"\"{raw.Replace("\"", "\\\"")}\""
            : raw;

    private sealed class Indent
    {
        public int Level { get; set; }
        public override string ToString() => new string(' ', Level * 2);
    }

    private sealed class LocalSchemaRegistry
    {
        private readonly Dictionary<Type, SchemaNode> _schemas = new();
        private readonly NullabilityInfoContext _nullCtx = new();

        public string GetSchemaName(Type t) => _schemas[t].Name;

        public void EnsureSchema(Type t)
        {
            if (_schemas.ContainsKey(t)) return;

            if (TryWriteInline(t, out _))
            {
                _schemas[t] = SchemaNode.ForInline(NameFor(t), t);
                return;
            }

            var node = SchemaNode.ForObject(NameFor(t), t);
            _schemas[t] = node;

            foreach (var p in GetSerializableProperties(t))
            {
                var pt = p.PropertyType;
                VisitTypeRecursive(pt);
            }
        }

        public IEnumerable<SchemaNode> EmitSchemasInDependencyOrder()
        {
            return _schemas.Values.OrderBy(s => s.Name, StringComparer.Ordinal);
        }

        private void VisitTypeRecursive(Type t)
        {
            if (TryUnwrapNullable(t, out var underlying))
                t = underlying;

            if (TryWriteInline(t, out _))
                return;

            if (t.IsEnum)
            {
                if (!_schemas.ContainsKey(t))
                    _schemas[t] = SchemaNode.ForEnum(NameFor(t), t);
                return;
            }

            if (IsArrayLike(t, out var itemType))
            {
                VisitTypeRecursive(itemType);
                if (!_schemas.ContainsKey(t))
                    _schemas[t] = SchemaNode.ForArray(NameFor(t), t, itemType);
                return;
            }

            if (IsDictionaryLike(t, out var valueType))
            {
                VisitTypeRecursive(valueType);
                if (!_schemas.ContainsKey(t))
                    _schemas[t] = SchemaNode.ForDictionary(NameFor(t), t, valueType);
                return;
            }

            EnsureSchema(t);
        }

        private static string NameFor(Type t)
        {
            if (!t.IsGenericType) return t.Name;
            var def = t.GetGenericTypeDefinition().Name;
            def = def[..def.IndexOf('`')];
            var args = string.Join("", t.GetGenericArguments().Select(a => a.Name));
            return def + args;
        }

        private static bool TryUnwrapNullable(Type t, out Type underlying)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                underlying = t.GetGenericArguments()[0];
                return true;
            }
            underlying = t;
            return false;
        }

        private static bool IsArrayLike(Type t, out Type itemType)
        {
            if (t.IsArray)
            {
                itemType = t.GetElementType()!;
                return true;
            }

            if (t != typeof(string) &&
                t.GetInterfaces().Concat(new[] { t })
                 .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                itemType = t.GetGenericArguments().FirstOrDefault()
                           ?? t.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];
                return true;
            }

            itemType = typeof(void);
            return false;
        }

        private static bool IsDictionaryLike(Type t, out Type valueType)
        {
            var dictIface = t.GetInterfaces().Concat(new[] { t })
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (dictIface is not null)
            {
                var keyType = dictIface.GetGenericArguments()[0];
                valueType = dictIface.GetGenericArguments()[1];
                if (keyType == typeof(string))
                    return true;
            }

            valueType = typeof(void);
            return false;
        }

        private static IEnumerable<PropertyInfo> GetSerializableProperties(Type t) =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanRead && p.GetMethod is { IsStatic: false });

        private static (string type, string? format) MapPrimitive(Type t)
        {
            if (t == typeof(string)) return ("string", null);
            if (t == typeof(bool)) return ("boolean", null);
            if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int)) return ("integer", "int32");
            if (t == typeof(uint) || t == typeof(long)) return ("integer", "int64");
            if (t == typeof(ulong)) return ("integer", null);
            if (t == typeof(float)) return ("number", "float");
            if (t == typeof(double) || t == typeof(decimal)) return ("number", "double");
            if (t == typeof(Guid)) return ("string", "uuid");
            if (t == typeof(DateTime) || t == typeof(DateTimeOffset)) return ("string", "date-time");
            if (t == typeof(TimeSpan)) return ("string", "duration"); // RFC 3339 duration-ish
            if (t == typeof(Uri)) return ("string", "uri");
            return ("string", null);
        }

        private static bool TryWriteInline(Type t, out InlineSchema inline)
        {
            inline = default;

            if (t.IsEnum) return false;

            if (t == typeof(string) || t.IsPrimitive || t == typeof(decimal) || t == typeof(Guid) ||
                t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(TimeSpan) || t == typeof(Uri))
            {
                var (type, format) = MapPrimitive(t);
                inline = new InlineSchema(type, format);
                return true;
            }

            if (IsArrayLike(t, out _) || IsDictionaryLike(t, out _))
                return false;

            return false;
        }

        private readonly struct InlineSchema(string type, string? format)
        {
            public string Type { get; } = type;
            public string? Format { get; } = format;
        }

        public abstract class SchemaNode
        {
            public string Name { get; }
            public Type ClrType { get; }

            protected SchemaNode(string name, Type clrType)
            {
                Name = name;
                ClrType = clrType;
            }

            public abstract string ToYaml(Indent indent);

            public static SchemaNode ForInline(string name, Type t) => new InlineNode(name, t);
            public static SchemaNode ForEnum(string name, Type t) => new EnumNode(name, t);
            public static SchemaNode ForArray(string name, Type t, Type itemType) => new ArrayNode(name, t, itemType);
            public static SchemaNode ForDictionary(string name, Type t, Type valueType) => new DictionaryNode(name, t, valueType);
            public static SchemaNode ForObject(string name, Type t) => new ObjectNode(name, t);
        }

        private sealed class InlineNode : SchemaNode
        {
            public InlineNode(string name, Type t) : base(name, t) { }

            public override string ToYaml(Indent indent)
            {
                var (type, format) = MapPrimitive(ClrType);
                var sb = new StringBuilder();
                sb.AppendLine($"{indent}{Name}:");
                sb.AppendLine($"{indent}  type: {type}");
                if (format is not null)
                    sb.AppendLine($"{indent}  format: {format}");
                return sb.ToString();
            }
        }

        private sealed class EnumNode : SchemaNode
        {
            public EnumNode(string name, Type t) : base(name, t) { }

            public override string ToYaml(Indent indent)
            {
                var names = Enum.GetNames(ClrType);
                var underlying = Enum.GetUnderlyingType(ClrType);
                var isStringEnum = ClrType.GetCustomAttributes().Any(a => a.GetType().Name is "EnumMemberAttribute" or "JsonStringEnumConverter");
                var sb = new StringBuilder();
                sb.AppendLine($"{indent}{Name}:");
                sb.AppendLine($"{indent}  type: string");
                sb.AppendLine($"{indent}  enum:");
                foreach (var n in names)
                    sb.AppendLine($"{indent}    - {n}");
                return sb.ToString();
            }
        }

        private sealed class ArrayNode : SchemaNode
        {
            private readonly Type _itemType;

            public ArrayNode(string name, Type t, Type itemType) : base(name, t)
            {
                _itemType = itemType;
            }

            public override string ToYaml(Indent indent)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{indent}{Name}:");
                sb.AppendLine($"{indent}  type: array");
                sb.AppendLine($"{indent}  items:");
                EmitTypeRefOrInline(sb, indent, _itemType, 2);
                return sb.ToString();
            }
        }

        private sealed class DictionaryNode : SchemaNode
        {
            private readonly Type _valueType;

            public DictionaryNode(string name, Type t, Type valueType) : base(name, t)
            {
                _valueType = valueType;
            }

            public override string ToYaml(Indent indent)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{indent}{Name}:");
                sb.AppendLine($"{indent}  type: object");
                sb.AppendLine($"{indent}  additionalProperties:");
                EmitTypeRefOrInline(sb, indent, _valueType, 2);
                return sb.ToString();
            }
        }

        private sealed class ObjectNode : SchemaNode
        {
            public ObjectNode(string name, Type t) : base(name, t) { }

            public override string ToYaml(Indent indent)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{indent}{Name}:");
                sb.AppendLine($"{indent}  type: object");

                var props = GetSerializableProperties(ClrType).ToArray();
                if (props.Length == 0) return sb.ToString();

                var required = new List<string>();
                foreach (var p in props)
                {
                    var ni = new NullabilityInfoContext().Create(p);
                    var isNullable = ni.ReadState == NullabilityState.Nullable;
                    if (!isNullable) required.Add(ToCamelCase(p.Name));
                }

                if (required.Count > 0)
                {
                    sb.AppendLine($"{indent}  required:");
                    foreach (var r in required.Distinct().OrderBy(x => x, StringComparer.Ordinal))
                        sb.AppendLine($"{indent}    - {r}");
                }

                sb.AppendLine($"{indent}  properties:");
                foreach (var p in props.OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    var propName = ToCamelCase(p.Name);
                    sb.AppendLine($"{indent}    {propName}:");
                    EmitTypeRefOrInline(sb, indent, p.PropertyType, 3, includeNullability: true);
                }

                return sb.ToString();
            }
        }

        private static void EmitTypeRefOrInline(
            StringBuilder sb,
            Indent indent,
            Type t,
            int extraLevels,
            bool includeNullability = false)
        {
            var nullable = false;
            if (TryUnwrapNullable(t, out var underlying))
            {
                t = underlying;
                nullable = true;
            }

            var baseIndent = new Indent { Level = indent.Level + extraLevels };

            if (TryWriteInline(t, out var inline))
            {
                if (nullable)
                {
                    sb.AppendLine($"{baseIndent}type:");
                    sb.AppendLine($"{baseIndent}  - {inline.Type}");
                    sb.AppendLine($"{baseIndent}  - \"null\"");
                    if (inline.Format is not null)
                        sb.AppendLine($"{baseIndent}format: {inline.Format}");
                }
                else
                {
                    sb.AppendLine($"{baseIndent}type: {inline.Type}");
                    if (inline.Format is not null)
                        sb.AppendLine($"{baseIndent}format: {inline.Format}");
                }
                return;
            }

            if (t.IsEnum)
            {
                if (nullable)
                {
                    sb.AppendLine($"{baseIndent}$ref: '#/components/schemas/{NameFor(t)}'");
                    sb.AppendLine($"{baseIndent}nullable: true");
                }
                else
                {
                    sb.AppendLine($"{baseIndent}$ref: '#/components/schemas/{NameFor(t)}'");
                }

                return;
            }

            if (IsArrayLike(t, out var itemType))
            {
                if (nullable)
                {
                    sb.AppendLine($"{baseIndent}type:");
                    sb.AppendLine($"{baseIndent}  - array");
                    sb.AppendLine($"{baseIndent}  - \"null\"");
                }
                else
                {
                    sb.AppendLine($"{baseIndent}type: array");
                }
                sb.AppendLine($"{baseIndent}items:");
                EmitTypeRefOrInline(sb, indent, itemType, extraLevels + 1);
                return;
            }

            if (IsDictionaryLike(t, out var valueType))
            {
                if (nullable)
                {
                    sb.AppendLine($"{baseIndent}type:");
                    sb.AppendLine($"{baseIndent}  - object");
                    sb.AppendLine($"{baseIndent}  - \"null\"");
                }
                else
                {
                    sb.AppendLine($"{baseIndent}type: object");
                }
                sb.AppendLine($"{baseIndent}additionalProperties:");
                EmitTypeRefOrInline(sb, indent, valueType, extraLevels + 1);
                return;
            }

            sb.AppendLine($"{baseIndent}$ref: '#/components/schemas/{NameFor(t)}'");
        }
    }
}
