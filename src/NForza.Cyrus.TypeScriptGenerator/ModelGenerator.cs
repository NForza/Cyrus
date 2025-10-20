using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Cyrus;

namespace NForza.Cyrus.TypeScriptGenerator;

internal class ModelGenerator(string modelAssemblyFile)
{
    internal (bool succeeded, string? model) GetModel()
    {
        if (!File.Exists(modelAssemblyFile))
        {
            Console.WriteLine($"Model assembly file not found: {modelAssemblyFile}");
            return (false, null);
        }
        var modelJson = GetModelJsonFromAssembly();
        if (string.IsNullOrEmpty(modelJson))
        {
            Console.WriteLine($"Model metadata not found in assembly: {modelAssemblyFile}");
            return (false, null);
        }
        return (true, modelJson);
    }

    private string GetModelJsonFromAssembly()
    {
        var jsonMetadata = ReadAssemblyMetadata(modelAssemblyFile, "cyrus-model")
            .Select(kv => kv.Value)
            .FirstOrDefault();

        return string.IsNullOrEmpty(jsonMetadata) ? "" : jsonMetadata;
    }

    static IReadOnlyList<(string Key, string Value)> ReadAssemblyMetadata(string assemblyPath, string keyValue)
    {
        using var fs = File.OpenRead(assemblyPath);
        using var pe = new PEReader(fs);
        var md = pe.GetMetadataReader();

        var asm = md.GetAssemblyDefinition();
        var result = new List<(string, string)>();

        foreach (var caHandle in asm.GetCustomAttributes())
        {
            var ca = md.GetCustomAttribute(caHandle);

            // Get attribute type full name without resolving anything
            string attrFullName = GetAttributeTypeFullName(md, ca);
            if (attrFullName != "System.Reflection.AssemblyMetadataAttribute")
            {
                continue;
            }

            // Decode the blob: prolog (0x0001) + SerString key + SerString value
            var br = md.GetBlobReader(ca.Value);
            ushort prolog = br.ReadUInt16(); // must be 0x0001
            if (prolog != 0x0001) continue;

            string key = br.ReadSerializedString();
            string value = br.ReadSerializedString();

            if (key == keyValue)
            {
                result.Add((key, value.DecompressFromBase64()));
            }
        }

        return result;

        static string GetAttributeTypeFullName(MetadataReader md, CustomAttribute ca)
        {
            var ctor = ca.Constructor;
            EntityHandle typeHandle = ctor.Kind switch
            {
                HandleKind.MemberReference => md.GetMemberReference((MemberReferenceHandle)ctor).Parent,
                HandleKind.MethodDefinition => md.GetMethodDefinition((MethodDefinitionHandle)ctor).GetDeclaringType(),
                _ => default
            };

            if (typeHandle.Kind == HandleKind.TypeReference)
            {
                var tr = md.GetTypeReference((TypeReferenceHandle)typeHandle);
                var ns = md.GetString(tr.Namespace);
                var name = md.GetString(tr.Name);
                return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
            }
            else if (typeHandle.Kind == HandleKind.TypeDefinition)
            {
                var td = md.GetTypeDefinition((TypeDefinitionHandle)typeHandle);
                var ns = md.GetString(td.Namespace);
                var name = md.GetString(td.Name);
                return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
            }
            return "";
        }
    }
}
