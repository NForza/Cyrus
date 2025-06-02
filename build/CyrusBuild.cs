using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;

class CyrusBuild : NukeBuild
{
    public static int Main() => Execute<CyrusBuild>(x => x.Compile);

    private static string cyrusSolutionFileName = "NForza.Cyrus.sln";
    private static AbsolutePath cyrusSolutionPath = RootDirectory / cyrusSolutionFileName;
    private static AbsolutePath examplesPath = RootDirectory / "examples";
    private static AbsolutePath massTransitExamplePath = examplesPath / "MassTransit" / "MassTransit.sln";
    private static AbsolutePath signalRExamplePath = examplesPath / "SignalR" / "CyrusSignalR" / "CyrusSignalR.sln";
    private static AbsolutePath tracksDemoExamplePath = examplesPath / "TracksDemo" / "TracksDemo.sln";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(cyrusSolutionPath)
            );
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var projects = new[]
            {
            cyrusSolutionPath,
            massTransitExamplePath,
            signalRExamplePath,
            tracksDemoExamplePath
        };

            foreach (var project in projects)
            {
                LogBanner(Path.GetFileNameWithoutExtension(project));

                DotNetTasks.DotNetBuild(s => s
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration));
            }
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(s => s
                .SetProjectFile(cyrusSolutionPath)
                .AddLoggers("trx")
                .SetResultsDirectory(RootDirectory / "test-results")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetPublish(s => s
                .SetProject(cyrusSolutionPath)
                .SetOutput(RootDirectory / "output")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    void LogBanner(string title)
    {
        var bannerWidth = 80;
        var line = new string('═', bannerWidth);
        var paddedTitle = $" Project: {title} ".PadLeft((bannerWidth + title.Length + 10) / 2).PadRight(bannerWidth);
              
        Log.Information("");
        Log.Information(line);
        Log.Information(paddedTitle);
        Log.Information(line);
        Log.Information("");
    }

}
