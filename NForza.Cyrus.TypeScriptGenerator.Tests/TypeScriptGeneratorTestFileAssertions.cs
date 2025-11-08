
using FluentAssertions;

namespace NForza.Cyrus.TypeScriptGenerator.Tests
{
    internal class TypeScriptGeneratorTestFileAssertions(string textContents)
    {
        internal void ThatContains(string text)
        {
            textContents.Should().Contain(text);
        }
    }
}