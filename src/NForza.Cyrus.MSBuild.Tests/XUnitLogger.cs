using Cyrus;
using Microsoft.Build.Framework;
using Xunit.Abstractions;

namespace NForza.Cyrus.MSBuild.Tests
{
    internal class XUnitLogger(ITestOutputHelper outputWindow) : ITaskLogger
    {
        public void LogErrorFromException(Exception exception, bool showStackTrace = false)
        {
            outputWindow.WriteLine(exception.ToString());
        }

        public void LogMessage(MessageImportance importance, string messageFormat, params object[] messageArgs)
        {
            if (messageArgs.Length > 0)
                outputWindow.WriteLine(string.Format(messageFormat, messageArgs));
            else
                outputWindow.WriteLine(messageFormat);
        }
    }
}