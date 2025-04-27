using System;
using Microsoft.Build.Framework;

namespace Cyrus
{
    internal interface ITaskLogger
    {
        void LogMessage(MessageImportance importance, string messageFormat, params object[] messageArgs);
        void LogErrorFromException(Exception exception, bool showStackTrace = false);
    }
}
