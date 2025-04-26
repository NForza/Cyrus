using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cyrus
{
    internal interface ITaskLogger
    {
        void LogMessage(MessageImportance importance, string messageFormat, params object[] messageArgs);
        void LogErrorFromException(Exception exception, bool showStackTrace = false);
    }

    internal class TaskLogger : ITaskLogger
    {
        private readonly TaskLoggingHelper _log;
        public TaskLogger(TaskLoggingHelper log)
        {
            _log = log;
        }
        public void LogMessage(MessageImportance importance, string messageFormat, params object[] messageArgs)
        {
            _log.LogMessage(importance, messageFormat, messageArgs);
        }
        public void LogErrorFromException(Exception exception, bool showStackTrace = false)
        {
            _log.LogErrorFromException(exception, showStackTrace);
        }
    }

    internal class ConsoleLogger : ITaskLogger
    {
        public void LogMessage(MessageImportance importance, string messageFormat, params object[] messageArgs)
        {
            Console.WriteLine(messageFormat, messageArgs);
        }
        public void LogErrorFromException(Exception exception, bool showStackTrace = false)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}
