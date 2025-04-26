using System;
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

        try
        {
            Logger.LogMessage(MessageImportance.Normal, "Verifying input parameters");

            if (!string.IsNullOrEmpty(AssemblyPath))
            {
                //run "dotnet <AssemblyPath> --generateModel --console"
                //Capture the output and create a memory stream
            }
            else
            if (ModelFile is null || !File.Exists(ModelFile))
            {
                Logger.LogMessage(MessageImportance.High, $"Input file {OutputFolder} does not exist.");
                return false;
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
                // Read the file as a Stream    
            }

            // Refactor this to use a stream
            return TypeScriptGenerator.Generate(ModelFile, OutputFolder, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogErrorFromException(ex, true);
            return false;
        }
    }
}
