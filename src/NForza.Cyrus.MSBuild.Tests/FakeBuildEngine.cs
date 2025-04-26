using System.Collections;
using Microsoft.Build.Framework;

namespace NForza.Cyrus.MSBuild.Tests;

sealed class FakeBuildEngine : IBuildEngine
{
    public readonly List<BuildErrorEventArgs> Errors = new();
    public readonly List<BuildWarningEventArgs> Warnings = new();
    public readonly List<BuildMessageEventArgs> Messages = new();
    public readonly List<CustomBuildEventArgs> CustomEvents = new();

    public void LogErrorEvent(BuildErrorEventArgs e) => Errors.Add(e);
    public void LogWarningEvent(BuildWarningEventArgs e) => Warnings.Add(e);
    public void LogMessageEvent(BuildMessageEventArgs e) => Messages.Add(e);
    public void LogCustomEvent(CustomBuildEventArgs e) => CustomEvents.Add(e);

    public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        => true;

    public bool ContinueOnError => false;
    public int LineNumberOfTaskNode => 0;
    public int ColumnNumberOfTaskNode => 0;
    public string ProjectFileOfTaskNode => "";
}