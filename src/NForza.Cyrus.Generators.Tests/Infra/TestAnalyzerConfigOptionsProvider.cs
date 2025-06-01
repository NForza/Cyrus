using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

public class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly AnalyzerConfigOptions _globalOptions;

    public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions)
    {
        _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
    }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _globalOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _globalOptions;

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;
}

public class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly Dictionary<string, string> _options;

    public TestAnalyzerConfigOptions(Dictionary<string, string> options)
    {
        _options = options;
    }

    public override bool TryGetValue(string key, out string value) => _options.TryGetValue(key, out value);
}
