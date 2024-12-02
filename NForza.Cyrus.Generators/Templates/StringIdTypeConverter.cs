using System;

#nullable enable

namespace % NamespaceName %;

public partial class % TypedIdName %TypeConverter : System.ComponentModel.TypeConverter
{
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
    {
        return value switch
        {
            string str => new % TypedIdName % (str),
            _ => base.ConvertFrom(context, culture, value)
        };
    }

    public override object? ConvertTo(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, System.Type destinationType) =>
        destinationType == typeof(string)
            ? ((% TypedIdName %?)value)?.Value.ToString() ?? string.Empty
            : base.ConvertTo(context, culture, value, destinationType);
}