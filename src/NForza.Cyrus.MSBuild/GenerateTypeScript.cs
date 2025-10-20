using System;
using System.IO;
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

            (bool succeeded, string errors, string model) = (true, string.Empty, string.Empty);
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
}

