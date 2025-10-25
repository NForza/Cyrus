using System.Reflection;

namespace NForza.Cyrus;

public static class AssemblyExtensions
{
    public static bool IsFrameworkAssembly(this Assembly assembly) => assembly.GetName().IsFrameworkAssembly();

    public static bool IsFrameworkOrCyrusAssembly(this Assembly assembly) => assembly.GetName().IsFrameworkOrCyrusAssembly();
}
