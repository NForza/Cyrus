using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cyrus
{
    internal class TaskLogger : ITaskLogger
    {
        private readonly TaskLoggingHelper log;
        public TaskLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }
        public void LogMessage(MessageImportance importance, string messageFormat, params object[] messageArgs)
        {
            log.LogMessage(importance, messageFormat, messageArgs);
        }
        public void LogErrorFromException(Exception exception, bool showStackTrace = false)
        {
            log.LogErrorFromException(exception, showStackTrace);
        }
    }
}
