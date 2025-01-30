#if DEBUG_ANALYZER
using System.Diagnostics;
#endif
namespace NForza.Cyrus.Generators;

public class IncrementalGeneratorBase
{
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
