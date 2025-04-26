using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cyrus;

public partial class GenerateTypeScript : Task
{
    [Required]
    public string ModelFile { get; set; } = string.Empty; 

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
            return TypeScriptGenerator.Generate(ModelFile, OutputFolder, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogErrorFromException(ex, true);
            return false;
        }
    }
}
