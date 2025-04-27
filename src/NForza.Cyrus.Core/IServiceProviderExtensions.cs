using System;
using System.IO;
using NForza.Cyrus.Model;

namespace NForza.Cyrus;

public static class IServiceProviderExtensions
{
    public static bool OutputCyrusModelOnly(this IServiceProvider serviceProvider, string[] args)
    {
        if (args.Length == 0 || args[0] != "--generateModel")
        {
            return false;
        }
        var generator = CyrusModel.Aggregate(serviceProvider);
        var fileName = args.Length > 1 ? args[1].Trim() : "cyrus.json";
        if (fileName == "--console")
        {
            Console.WriteLine(generator.AsJson());
            return true;
        }
        File.WriteAllText(fileName, generator.AsJson());
        return true;
    }
}
