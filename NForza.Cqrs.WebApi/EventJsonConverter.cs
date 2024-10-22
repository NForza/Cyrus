//using System;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace NForza.Cqrs.Events;

//[JsonConverter(typeof(EventJsonConverter))]
//public partial interface IEvent
//{

//}

//public class EventJsonConverter : JsonConverter<IEvent>
//{
//    public override IEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null;

//    public override void Write(Utf8JsonWriter writer, IEvent value, JsonSerializerOptions options)
//    {
//        writer.WriteStartObject();
//        writer.WriteString("$type", value.GetType().Name);
//        writer.WritePropertyName("payload");
//        writer.WriteStartObject();
//        var allProps = value.GetType().GetProperties();
//        foreach (var prop in allProps)
//        {
//            if (!prop.CanRead)
//                continue;

//            var propValue = prop.GetValue(value);
//            switch (propValue)
//            {
//                case int nr:
//                    writer.WriteNumber(prop.Name, nr);
//                    break;
//                case long nr:
//                    writer.WriteNumber(prop.Name, nr);
//                    break;
//                case float nr:
//                    writer.WriteNumber(prop.Name, nr);
//                    break;
//                case decimal nr:
//                    writer.WriteNumber(prop.Name, nr);
//                    break;
//                case double nr:
//                    writer.WriteNumber(prop.Name, nr);
//                    break;
//                case DateTime dt:
//                    writer.WriteString(prop.Name, dt);
//                    break;
//                case bool b:
//                    writer.WriteBoolean(prop.Name, b);
//                    break;
//                default:
//                    writer.WriteString(prop.Name, propValue?.ToString());
//                    break;
//            }
//        }
//        writer.WriteEndObject();
//        writer.WriteEndObject();
//    }
//}