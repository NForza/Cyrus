namespace NForza.Cyrus.TypescriptGenerate;

public static class TypeScriptStringExtensions
{
    public static bool IsBuiltInTypeScriptType(this string name)
    {
        return name switch
        {
            "string" => true,
            "number" => true,
            "boolean" => true,
            "any" => true,
            "void" => true,
            "null" => true,
            "undefined" => true,
            "never" => true,
            "object" => true,
            _ => false
        };
    }
}
