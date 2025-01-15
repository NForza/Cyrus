namespace NForza.Cyrus;

public static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }
        str = char.ToLowerInvariant(str[0]) + str.Substring(1);
        return str;
    }
}