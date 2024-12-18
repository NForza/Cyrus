using System.Reflection;

namespace NForza.Generators;

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
