using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;

namespace NForza.TypedIds;

public class TypedIdConverter(Type stronglyTypedIdType) : TypeConverter
{
    private static readonly ConcurrentDictionary<Type, TypeConverter> ActualConverters = new();

    private readonly TypeConverter innerConverter = ActualConverters.GetOrAdd(stronglyTypedIdType, CreateActualConverter);

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        innerConverter.CanConvertFrom(context, sourceType);
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        innerConverter.CanConvertTo(context, destinationType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        innerConverter.ConvertFrom(context, culture, value);
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
        innerConverter.ConvertTo(context, culture, value, destinationType);


    private static TypeConverter CreateActualConverter(Type stronglyTypedIdType)
    {
        if (!TypedIdHelper.IsStronglyTypedId(stronglyTypedIdType, out var idType))
            throw new InvalidOperationException($"The type '{stronglyTypedIdType}' is not a strongly typed id");

        var actualConverterType = typeof(TypedIdConverter<>).MakeGenericType(idType);
        return (TypeConverter)Activator.CreateInstance(actualConverterType, stronglyTypedIdType)!;
    }
}

public class TypedIdConverter<TValue>(Type type) : TypeConverter
    where TValue : notnull
{
    private static readonly TypeConverter IdValueConverter = GetIdValueConverter();

    private static TypeConverter GetIdValueConverter()
    {
        var converter = TypeDescriptor.GetConverter(typeof(TValue));
        if (!converter.CanConvertFrom(typeof(string)))
            throw new InvalidOperationException(
                $"Type '{typeof(TValue)}' doesn't have a converter that can convert from string");
        return converter;
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string)
            || sourceType == typeof(TValue)
            || base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string)
            || destinationType == typeof(TValue)
            || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            value = IdValueConverter?.ConvertFrom(s) ?? throw new InvalidOperationException($"No IdValueConverter for {context!.Instance!.GetType().FullName}");
        }

        if (value is TValue idValue)
        {
            var factory = TypedIdHelper.GetFactory<TValue>(type);
            return factory(idValue);
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(value);

        var stronglyTypedId = (TypedId<TValue>)value;
        TValue idValue = stronglyTypedId.Value;
        if (destinationType == typeof(string))
            return idValue.ToString()!;
        if (destinationType == typeof(TValue))
            return idValue;
        return base.ConvertTo(context, culture, value, destinationType);
    }
}

