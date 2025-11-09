using System.IO.Abstractions.TestingHelpers;

namespace NForza.Cyrus.TypeScriptGenerator.Tests.Infra
{
    internal class TypeScriptGeneratorTestResult(MockFileSystem mockFileSystem)
    {
        public TypeScriptGeneratorTestAssertions Should() => new(mockFileSystem);
    }
}