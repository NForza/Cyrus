using Cyrus;
using FluentAssertions;
using Xunit.Abstractions;

namespace NForza.Cyrus.MSBuild.Tests;

public class GenerateTypeScriptTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public void DemoApp_Model_File_Should_Generate_Output()
    {
        // arrange
        var task = new GenerateTypeScript
        {
            ModelFile = "ModelFiles/DemoApp.WebApi.json",
            OutputFolder = Path.GetTempPath(),
        };
        task.UseLogger(new XUnitLogger(outputWindow));
        var engine = new FakeBuildEngine();
        task.BuildEngine = engine;

        // act
        var success = task.Execute();

        // assert
        success.Should().BeTrue();
        engine.Errors.Should().BeEmpty();   
        File.Exists(Path.Combine(task.OutputFolder, "CustomerId.ts")).Should().BeTrue();
        File.Exists(Path.Combine(task.OutputFolder, "AllCustomersQuery.ts")).Should().BeTrue();
    }

    [Fact]
    public void SimpleWebApp_Model_File_Should_Generate_Output()
    {
        // arrange
        var task = new GenerateTypeScript
        {
            ModelFile = "ModelFiles/DemoApp.WebApi.json",
            OutputFolder = Path.GetTempPath(),
        };
        task.UseLogger(new XUnitLogger(outputWindow));
        var engine = new FakeBuildEngine();
        task.BuildEngine = engine;

        // act
        var success = task.Execute();

        // assert
        success.Should().BeTrue();
        engine.Errors.Should().BeEmpty();
        File.Exists(Path.Combine(task.OutputFolder, "CustomerId.ts")).Should().BeTrue();
        File.Exists(Path.Combine(task.OutputFolder, "UpdateCustomerCommand.ts")).Should().BeTrue();
    }

    [Fact]
    public void DemoApp_AssemblyPath_Should_Generate_Output()
    {
        // arrange
        var task = new GenerateTypeScript
        {
            AssemblyPath = @"C:\dev\Cyrus\examples\DemoApp\DemoApp.WebApi\bin\Debug\net9.0\DemoApp.WebApi.dll",
            OutputFolder = Path.GetTempPath(),
        };
        task.UseLogger(new XUnitLogger(outputWindow));
        var engine = new FakeBuildEngine();
        task.BuildEngine = engine;

        // act
        var success = task.Execute();

        // assert
        success.Should().BeTrue();
        engine.Errors.Should().BeEmpty();
        File.Exists(Path.Combine(task.OutputFolder, "CustomerId.ts")).Should().BeTrue();
        File.Exists(Path.Combine(task.OutputFolder, "UpdateCustomerCommand.ts")).Should().BeTrue();
    }
}
