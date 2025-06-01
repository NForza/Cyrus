using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NForza.Cyrus.Generators.Tests.Infra
{
    class InMemoryAdditionalText(string path, SourceText text) : AdditionalText
    {
        public override string Path => path;
        public override SourceText GetText(CancellationToken cancellationToken = default) => text;
    }

}