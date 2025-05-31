using FluentAssertions;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class TestProjectTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Async_CommandHandler_With_Result_And_Messages_Should_Generate_Valid_CommandDispatcher_Method()
    {
        var source = @"
                using System;
                using NForza.Cyrus.Abstractions;                

                namespace Test;

                [Command]
                public record struct NewTrackCommand(Guid TrackId);
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .InTestProject()
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().BeEmpty();
        compilerOutput.Should().NotHaveErrors();
        generatedSyntaxTrees.Should().BeEmpty();
    }
}
