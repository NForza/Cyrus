using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cyrus;

public partial class GenerateTypeScript : Task
{
    public string ModelFile { get; set; } = string.Empty;
    public string AssemblyPath { get; set; } = string.Empty;

    [Required]
    public string OutputFolder { get; set; } = string.Empty;

    const string TypeName = "Cyrus.TypeScriptGenerator";
    const string MethodName = "GetModelAsJson";

    internal ITaskLogger Logger => _logger ??= new TaskLogger(this.Log);
    internal ITaskLogger? _logger = null;

    internal void UseLogger(ITaskLogger logger)
    {
        _logger = logger;
    }

    public override bool Execute()
    {
        Logger.LogMessage(MessageImportance.High, "Starting TypeScript generation...");

        Stream? modelStream = null;
        try
        {
            Logger.LogMessage(MessageImportance.Normal, "Verifying input parameters");

            if (!string.IsNullOrEmpty(AssemblyPath))
            {
                Logger.LogMessage(MessageImportance.Normal, $"Verifying assembly at {AssemblyPath}");
                if (!File.Exists(AssemblyPath))
                {
                    Logger.LogMessage(MessageImportance.High, $"Assembly file {AssemblyPath} does not exist.");
                    return false;
                }
                (bool succeeded, string errors, string model) = GetModelFromAssembly();
                if (!succeeded)
                {
                    Logger.LogMessage(MessageImportance.High, $"Failed to generate model from assembly {AssemblyPath}.");
                    Logger.LogMessage(MessageImportance.High, errors);
                    return false;
                }

                modelStream = new MemoryStream(Encoding.UTF8.GetBytes(model));
            }
            else
            {
                Logger.LogMessage(MessageImportance.Normal, $"No AssemblyPath specified, checking ModelFile");
                ModelFile = Path.GetFullPath(ModelFile);
                if (ModelFile is null || !File.Exists(ModelFile))
                {
                    Logger.LogMessage(MessageImportance.High, $"Input file {ModelFile} does not exist.");
                    return false;
                }
                Logger.LogMessage(MessageImportance.Normal, $"ModelFile {ModelFile} found.");
                modelStream = File.OpenRead(ModelFile);
            }
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

            if (modelStream != null)
            {
                return TypeScriptGenerator.Generate(modelStream, OutputFolder, Logger);
            }
            else
            {
                Logger.LogMessage(MessageImportance.High, $"Unknown Stream.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.LogErrorFromException(ex, true);
            return false;
        }
    }

    private (bool succeeded, string errors, string model) GetModelFromAssembly()
    {
        var bytes = File.ReadAllBytes(AssemblyPath);
        var asm = Assembly.Load(bytes);
        try
        {
            var type = asm.GetType(TypeName, throwOnError: true, ignoreCase: false);
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var method = type.GetMethod(MethodName, flags) ?? throw new MissingMethodException(TypeName, MethodName);

            Logger.LogMessage(MessageImportance.Normal, $"Invoking {TypeName}.{MethodName}()");

            var result = method.Invoke(null, null);
            return (true, "", result?.ToString() ?? "");
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, showStackTrace: true);
            return (false, ex.ToString(), "");
        }
    }
}
