#if DEBUG_ANALYZER
using System.Diagnostics;
#endif
using System.Reflection;

namespace NForza.Cyrus.Generators;

public class IncrementalGeneratorBase
{
    protected TemplateEngine TemplateEngine = new(Assembly.GetExecutingAssembly(), "Templates");

    public void DebugThisGenerator(bool debug)
    {
#if DEBUG_ANALYZER
        if (!Debugger.IsAttached && debug)
        {
            Debugger.Launch();
        }
#endif
    }

}
