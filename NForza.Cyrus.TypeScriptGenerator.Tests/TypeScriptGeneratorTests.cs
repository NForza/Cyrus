using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.TypeScriptGenerator.Tests.Infra;

namespace NForza.Cyrus.TypeScriptGenerator.Tests;

public class TypeScriptGeneratorTests
{
    [Fact]
    public void Command_In_Model_Should_Generate_TS_Code()
    {
        CyrusMetadata metadata = new CyrusMetadata()
        {
            Commands = [
                new ModelTypeDefinition(
                    "TestCommand",
                    "NS.TestCommand",
                    "Test Command",
                    [
                        new ModelPropertyDefinition("Id","Guid", false, false)
                    ],
                    [],
                    isCollection: false,
                    isNullable: false
                )
            ]
        };
        var testResult =  TypeScriptGeneratorTest.For(metadata);
        testResult.Should().HaveFile("TestCommand.ts").ThatContains("export interface TestCommand");
    }

    [Fact]
    public void Guid_In_Model_Should_Generate_TS_Code()
    {
        CyrusMetadata metadata = new CyrusMetadata()
        {
            Guids = [ "CustomerId" ]
        };
        var testResult = TypeScriptGeneratorTest.For(metadata);
        testResult.Should().HaveFile("CustomerId.ts").ThatContains("export type CustomerId = string");
    }
}
