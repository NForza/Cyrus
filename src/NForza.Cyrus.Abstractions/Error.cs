using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NForza.Cyrus.Abstractions;

public sealed class Error
{
    public ErrorType ErrorType { get; private set; } = ErrorType.None;
    public string Code { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Field { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public Error[] InnerErrors { get; private set; } = Array.Empty<Error>();
    public Exception InnerException { get; private set; }
    public string Tag { get; private set; }

    public Error WithException(Exception exception)
    {
        InnerException = exception;
        return this;
    }

    public Error WithErrors(IEnumerable<Error> errors)
    {
        InnerErrors = errors.ToArray();
        return this;
    }

    public Error WithSource<TSource>()
        => WithSource(typeof(TSource));

    public Error WithSource(object source)
    {
        if (source is null)
        {
            return this;
        }

        if (source is string sourceString)
        {
            Source = sourceString;
        }
        else if (source is Type sourceType)
        {
            Source = sourceType.Name.Split('`')[0];
        }
        else
        {
            Source = source.GetType().Name.Split('`')[0];
        }

        return this;
    }

    public Error WithTag(string tag)
    {
        Tag = tag;
        return this;
    }

    public Error WithErrorType(ErrorType errorType)
    {
        if (errorType == ErrorType.None)
        {
            throw new ArgumentException("Error type cannot be None.", nameof(errorType));
        }

        ErrorType = errorType;
        return this;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dictionary = new Dictionary<string, object>();
    
        AddIfNotEmpty(dictionary, "error-type", ErrorType != ErrorType.None, ErrorType.ToString());
        AddIfNotEmpty(dictionary, "message", !string.IsNullOrWhiteSpace(Message), Message);
        AddIfNotEmpty(dictionary, "code", !string.IsNullOrWhiteSpace(Code), Code);
        AddIfNotEmpty(dictionary, "source", !string.IsNullOrWhiteSpace(Source), Source);
    
        AddInnerErrors(dictionary);
    
        if (InnerException is not null)
        {
            dictionary.Add("exception-type", InnerException.GetType().Name);
            dictionary.Add("exception-message", InnerException.Message);
        }
    
        AddIfNotEmpty(dictionary, "tag", !string.IsNullOrWhiteSpace(Tag), Tag);
    
        return dictionary;
    }
    
    private void AddIfNotEmpty(Dictionary<string, object> dict, string key, bool condition, object value)
    {
        if (condition)
        {
            dict.Add(key, value);
        }
    }
    
    private void AddInnerErrors(Dictionary<string, object> dict)
    {
        if (InnerErrors is null || InnerErrors.Length == 0)
            return;
    
        for (int i = 0; i < InnerErrors.Length; i++)
        {
            AddIfNotEmpty(dict, $"error-{i + 1}-code", !string.IsNullOrWhiteSpace(InnerErrors[i].Code), InnerErrors[i].Code);
            AddIfNotEmpty(dict, $"error-{i + 1}-message", !string.IsNullOrWhiteSpace(InnerErrors[i].Message), InnerErrors[i].Message);
        }
    }

    public static Error Create(string message, ErrorType errorType = ErrorType.Invalid)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        if (errorType == ErrorType.None)
        {
            throw new ArgumentException("Error type cannot be None.", nameof(errorType));
        }

        return new Error()
        {
            Message = message,
            ErrorType = errorType
        };
    }

    public static Error Create(string message, string code, ErrorType errorType = ErrorType.Invalid)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        if (errorType == ErrorType.None)
        {
            throw new ArgumentException("Error type cannot be None.", nameof(errorType));
        }

        return new Error()
        {
            Message = message,
            Code = code,
            ErrorType = errorType
        };
    }

    public static Error Create(string message, string code, string field, ErrorType errorType = ErrorType.Invalid)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        if (errorType == ErrorType.None)
        {
            throw new ArgumentException("Error type cannot be None.", nameof(errorType));
        }

        return new Error()
        {
            Message = message,
            Code = code,
            ErrorType = errorType,
            Field = field,
        };
    }


    internal static class Factory
    {
        private readonly static ConcurrentBag<string> _endings = new ConcurrentBag<string>
        {
            "_COMMAND",
            "_QUERY",
            "_COMMAND_HANDLER",
            "_QUERY_HANDLER",
            "_HANDLER",
            "_VALIDATOR"
        };

        public static Error Unauthorized() => Create("User is not authenticated.", ErrorType.Unauthorized);
        public static Error UnAuthorized<T>(int errorId = 0) => CreateFor<T>("User is not authenticated.", ErrorType.Unauthorized, errorId);

        public static Error Forbidden<T>(int errorId = 0) => CreateFor<T>("Insufficient permissions to perform the operation.", ErrorType.Forbidden, errorId);
 
        public static Error Failure<T>(Exception exception, int errorId = 0)
        {
            var errorType = ErrorType.Failure;

            switch (exception)
            {
                case TimeoutException:
                    errorType = ErrorType.Timeout;
                    break;

                case TaskCanceledException:
                case OperationCanceledException:
                    errorType = ErrorType.Canceled;
                    break;
            }

            return CreateFor<T>("An unexpected error occurred while processing the request.", errorType).WithException(exception);
        }

        public static Error Invalid<T>(string message, int errorId = 0) => CreateFor<T>(message, ErrorType.Invalid, errorId);

        public static Error CreateFor<T>(string message, ErrorType errorType = ErrorType.Invalid, int errorId = 0)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            if (errorType == ErrorType.None)
            {
                throw new ArgumentException("Error type cannot be None.", nameof(errorType));
            }

            var type = typeof(T);
            var typeName = type.Name.Split('`')[0];

            if (type.IsClass && type.DeclaringType != null && (typeName == "Command" || typeName == "Query" || typeName == "Handler" || typeName == "Validator"))
            {
                typeName = type.DeclaringType.Name.Split('`')[0] + typeName;
            }

            var formattedCommand = Regex.Replace(typeName, "([A-Z])", "_$1").ToUpper().TrimStart('_');

            var ending = _endings.FirstOrDefault(x => formattedCommand.EndsWith(x));
            if (ending is not null)
            {
                formattedCommand = formattedCommand.Substring(0, formattedCommand.Length - ending.Length);
            }

            return new Error
            {
                Message = message,
                Code = $"ERR.{formattedCommand}.{(int)errorType}.{errorId}",
                ErrorType = errorType
            };
        }
    }
}
