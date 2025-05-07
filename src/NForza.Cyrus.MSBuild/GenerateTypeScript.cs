using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cyrus;

public partial class GenerateTypeScript : Task
{
    [Required]
    public string ToolPath { get; set; } = string.Empty;

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
        if (string.IsNullOrEmpty(OutputFolder))
        {
            Logger.LogMessage(MessageImportance.Normal, "OutputFolder for TypeScript Generation is not specified. Skipping.");
            return true;
        }

        Logger.LogMessage(MessageImportance.High, "Starting TypeScript generation...");

        try
        {
            Logger.LogMessage(MessageImportance.Normal, "Verifying input parameters");

            //if (!string.IsNullOrEmpty(AssemblyPath))
            //{
                Logger.LogMessage(MessageImportance.Normal, $"Verifying assembly at {AssemblyPath}");
                if (!File.Exists(AssemblyPath))
                {
                    Logger.LogMessage(MessageImportance.High, $"Assembly file {AssemblyPath} does not exist.");
                    return false;
                }
            //}
            //else
            //{
            //    Logger.LogMessage(MessageImportance.Normal, $"No AssemblyPath specified, checking ModelFile");
            //    ModelFile = Path.GetFullPath(ModelFile);
            //    if (ModelFile is null || !File.Exists(ModelFile))
            //    {
            //        Logger.LogMessage(MessageImportance.High, $"Input file {ModelFile} does not exist.");
            //        return false;
            //    }
            //    Logger.LogMessage(MessageImportance.Normal, $"ModelFile {ModelFile} found.");
            //    modelStream = File.OpenRead(ModelFile);
            //}
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

            (bool succeeded, string errors, string model) = GenerateTypeScriptFromAssembly();
            if (!succeeded)
            {
                Logger.LogMessage(MessageImportance.High, $"Failed to generate model from assembly {AssemblyPath}.");
                Logger.LogMessage(MessageImportance.High, errors);
                return false;
            }
            //if (modelStream != null)
            //{
            //    return TypeScriptGenerator.Generate(modelStream, OutputFolder, Logger);
            //}
            //else
            //{
            //    Logger.LogMessage(MessageImportance.High, $"Unknown Stream.");
            //    return false;
            //}

            Logger.LogMessage(MessageImportance.Normal, $"TypeScript generation succeeded. Output: {model}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogErrorFromException(ex, true);
            return false;
        }
    }

    private (bool succeeded, string errors, string model) GenerateTypeScriptFromAssembly()
    {
        var typeScriptGeneratorPath = Path.GetFullPath(ToolPath);
        if (!File.Exists(typeScriptGeneratorPath))
        {
            Logger.LogMessage(MessageImportance.High, $"TypeScript generator {typeScriptGeneratorPath} does not exist.");
            return (false, "TypeScript generator not found", string.Empty);
        }

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{typeScriptGeneratorPath}\" --path \"{AssemblyPath}\" --output \"{OutputFolder}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            if (!process.WaitForExit(1000))
                throw new TimeoutException("TypeScript generation timed out after 1 second.");

            var succeeded = process.ExitCode == 0;
            if (!succeeded)
            {
                Logger.LogMessage(MessageImportance.High, $"TypeScript generator exited with code {process.ExitCode}.");
            }

            return (succeeded, error.Trim(), output.Trim());
        }
        catch (Exception ex)
        {
            Logger.LogMessage(MessageImportance.High, $"Failed to run TypeScript generator: {ex.Message}");
            return (false, ex.Message, string.Empty);
        }
    }
}
