using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cyrus;

public partial class GenerateTypeScript : Task
{
    [Required]
    public string ToolPath { get; set; } = string.Empty;

    public bool CleanOutputFolder { get; set; } = false;

    public string ModelFile { get; set; } = string.Empty;
    public string AssemblyPath { get; set; } = string.Empty;
    public string OutputFolder { get; set; } = string.Empty;

    internal ITaskLogger Logger => _logger ??= new TaskLogger(this.Log);
    internal ITaskLogger? _logger = null;

    internal void UseLogger(ITaskLogger logger)
    {
        _logger = logger;
    }

    public override bool Execute()
    {
        Logger.LogMessage(MessageImportance.Normal, "Verifying input parameters");
        if (string.IsNullOrEmpty(OutputFolder))
        {
            Logger.LogMessage(MessageImportance.Normal, "OutputFolder for TypeScript Generation is not specified. Skipping.");
            return true;
        }
        var typeScriptGeneratorPath = Path.GetFullPath(ToolPath);
        if (!File.Exists(typeScriptGeneratorPath))
        {
            Logger.LogMessage(MessageImportance.High, $"TypeScript generator {typeScriptGeneratorPath} does not exist.");
            return true;
        }
        if (!File.Exists(AssemblyPath))
        {
            Logger.LogMessage(MessageImportance.High, $"Assembly file {AssemblyPath} does not exist.");
            return false;
        }
        Logger.LogMessage(MessageImportance.High, "Starting TypeScript generation...");

        try
        {
            var prepareSucceeded = PrepareOutputDirectory();
            if (!prepareSucceeded)
            {
                return false;
            }

            (bool succeeded, string errors, string model) = GetModelJsonFromAssembly();
            if (!succeeded)
            {
                Logger.LogMessage(MessageImportance.High, $"Failed to generate model from assembly {AssemblyPath}.");
                Logger.LogMessage(MessageImportance.High, errors);
                return false;
            }
            Logger.LogMessage(MessageImportance.Normal, $"TypeScript generation succeeded. Output: {model}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogErrorFromException(ex, true);
            return false;
        }
    }

    private bool PrepareOutputDirectory()
    {
        if (!Directory.Exists(OutputFolder))
        {
            Logger.LogMessage(MessageImportance.High, $"Output folder {OutputFolder} does not exist. Trying to create now");
            try
            {
                DirectoryInfo folder = Directory.CreateDirectory(OutputFolder);
                Logger.LogMessage(MessageImportance.Normal, $"Output folder {OutputFolder} created.");
            }
            catch
            {
                Logger.LogMessage(MessageImportance.High, $"Output folder {OutputFolder} could not be created.");
                return false;
            }
        }
        else
        {
            if (CleanOutputFolder)
            {
                Logger.LogMessage(MessageImportance.Normal, $"Cleaning output folder {OutputFolder}.");
                try
                {
                    Directory.Delete(OutputFolder, true);
                    Directory.CreateDirectory(OutputFolder);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(MessageImportance.High, $"Output folder {OutputFolder} could not be cleaned. {ex.Message}");
                    return false;
                }
            }
            else
            {
                Logger.LogMessage(MessageImportance.Normal, $"Output folder {OutputFolder} already exists and skipping cleanup.");
            }
        }

        return true;
    }

    private (bool succeeded, string errors, string model) GetModelJsonFromAssembly()
    {
        try
        {
            var jsonMetadata = ReadAssemblyMetadata(AssemblyPath, "cyrus-model")
                .Select(kv => kv.Value)
                .FirstOrDefault();
            Logger.LogMessage(MessageImportance.High, $"Model JSON: {jsonMetadata}");

            return (true, "", jsonMetadata);
        }
        catch (Exception ex)
        {
            Logger.LogMessage(MessageImportance.High, $"Failed to run TypeScript generator: {ex.Message}");
            return (false, ex.Message, string.Empty);
        }
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
                result.Add((key, value));
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
