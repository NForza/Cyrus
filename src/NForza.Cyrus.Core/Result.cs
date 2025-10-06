using System;
using System.Threading.Tasks;

namespace Framework.Core.Domain.Results
{
    public class Result
    {
        public static implicit operator Result(Error error) => Failure(error);

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; } = new Error();

        #region [ Constructors ]

        protected Result()
        {
            IsSuccess = true;
        }

        protected Result(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);

            if (error.ErrorType == ErrorType.None)
            {
                throw new ArgumentException("Error must have a type.", nameof(error));
            }

            IsSuccess = false;
            Error = error;
        }

        #endregion

        #region [ Creators ]

        public static Result Success() => new Result();
        public static Result<TValue> Success<TValue>(TValue value) where TValue : class => new Result<TValue>(value);
        public static Task<Result<TValue>> SuccessAsync<TValue>(TValue value) where TValue : class => Task.FromResult(new Result<TValue>(value));
        public static Result Failure(Error error) => new Result(error);
        public static Result<TValue> Failure<TValue>(Error error) where TValue : class => new Result<TValue>(error);


        #endregion
    }

    public class Result<TValue> : Result
        where TValue : class
    {
        public static implicit operator Result<TValue>(TValue value) => new(value);
        public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

        private readonly TValue? _value;
        public TValue? Value => IsFailure ? null : _value ?? throw new InvalidOperationException("Value is null despite the operation being marked as successful.");

        #region [ Constructors ]

        internal Result(TValue value) : base()
        {
            ArgumentNullException.ThrowIfNull(value);

            _value = value;
        }

        public Result(Error error) : base(error)
        {
        }

        #endregion
    }
}
