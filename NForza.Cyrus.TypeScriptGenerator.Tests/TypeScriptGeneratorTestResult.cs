using System.IO.Abstractions.TestingHelpers;

namespace NForza.Cyrus.TypeScriptGenerator.Tests
{
    internal class TypeScriptGeneratorTestResult(MockFileSystem mockFileSystem)
    {
        public TypeScriptGeneratorTestAssertions Should() => new(mockFileSystem);
    }
}