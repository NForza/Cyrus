using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cyrus;

public partial class GenerateTypeScript : Task
{
    public string ModelFile { get; set; } = string.Empty;
    public string AssemblyPath { get; set; } = string.Empty;

    [Required]
    public string OutputFolder { get; set; } = string.Empty;

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
            }
            else
            {
                if (ModelFile is null || !File.Exists(ModelFile))
                {
                    Logger.LogMessage(MessageImportance.High, $"Input file {OutputFolder} does not exist.");
                    return false;
                }
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
                modelStream = File.OpenRead(ModelFile);
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
        var psi = new ProcessStartInfo("dotnet", $"\"{AssemblyPath}\" --generateModel --console")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        var p = Process.Start(psi);
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            Log.LogError($"dotnet exited with code {p.ExitCode}");
            return (false, "", "");
        }
        return (true, "", "");
    }
}
