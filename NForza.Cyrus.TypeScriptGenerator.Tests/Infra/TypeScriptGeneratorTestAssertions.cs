using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;

namespace NForza.Cyrus.TypeScriptGenerator.Tests.Infra
{
    public class TypeScriptGeneratorTestAssertions(MockFileSystem mockFileSystem)
    {
        internal TypeScriptGeneratorTestFileAssertions HaveFile(string fileName)
        {
            var filePath = mockFileSystem.AllFiles.FirstOrDefault(f => f.EndsWith(fileName));
            filePath.Should().NotBeNull($"Expected file '{fileName}' to exist, but it does not.");
            return new TypeScriptGeneratorTestFileAssertions(mockFileSystem.GetFile(filePath).TextContents);
        }
    }
}