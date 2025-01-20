using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NForza.Cyrus.TypescriptGenerate.Model;
using Scriban;

namespace NForza.Cyrus.TypescriptGenerate;

public partial class GenerateTypeScriptTask : Task
{
    [Required]
    public string MetadataFile { get; set; } = string.Empty; 

    [Required]
    public string OutputFolder { get; set; } = string.Empty;

    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.High, "Starting TypeScript generation...");

        try
        {
            TypeScriptGenerator.Generate(MetadataFile, OutputFolder);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }
}
