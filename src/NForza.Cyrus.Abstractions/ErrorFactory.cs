using System;

namespace NForza.Cyrus.Abstractions;

public static class ErrorFactory<T>
{
    public static Error NotFound(string message, int errorId = 0)
    {
        return Error.Factory.CreateFor<T>(message, ErrorType.NotFound, errorId);
    }

    public static Error NotFound<TEntity>(int errorId = 0)
    {
        var entityName = typeof(TEntity).Name;
        return Error.Factory.CreateFor<T>($"The requested '{entityName}' could not be found.", ErrorType.NotFound, errorId);
    }

    public static Error NotFound<TEntity>(Guid id, int errorId = 0)
    {
        var entityName = typeof(TEntity).Name;
        return Error.Factory.CreateFor<T>($"The requested '{entityName}' could not be found. (Id: '{id}')", ErrorType.NotFound, errorId);
    }

    public static Error NotFound<TEntity>(string fieldName, string value, int errorId = 0)
    {
        var entityName = typeof(TEntity).Name;
        return Error.Factory.CreateFor<T>($"The requested '{entityName}' could not be found. ({fieldName}: '{value}')", ErrorType.NotFound, errorId);
    }

    public static Error EntityAlreadyExists<TEntity>(int errorId = 0)
    {
        var entityName = typeof(TEntity).Name;
        return Error.Factory.CreateFor<T>($"The requested '{entityName}' already exists.", ErrorType.Conflict, errorId);
    }

    public static Error EntityAlreadyExists<TEntity>(string fieldName, string duplicateValue, int errorId = 0)
    {
        var entityName = typeof(TEntity).Name;
        return Error.Factory.CreateFor<T>($"The requested '{entityName}' already exists. ({fieldName}: '{duplicateValue}')", ErrorType.Conflict, errorId);
    }

    public static Error NotOwnedByUser(object entity, int errorId = 0)
    {
        var entityName = entity.GetType().Name;
        return Error.Factory.CreateFor<T>($"The requested '{entityName}' does not belong to the current user.", ErrorType.Forbidden, errorId);
    }

    public static Error Conflict(string message, int errorId = 0)
    {
        return Error.Factory.CreateFor<T>(message, ErrorType.Conflict, errorId);
    }

    public static Error Failure(Exception exception, int errorId = 0)
    {
        return Error.Factory.Failure<T>(exception, errorId);
    }

    public static Error Failure(string message, int errorId = 0)
    {
        return Error.Factory.CreateFor<T>(message, ErrorType.Failure, errorId);
    }

    public static Error Failure(string message, Exception exception, int errorId = 0)
    {
        return Error.Factory.CreateFor<T>(message, ErrorType.Failure, errorId).WithException(exception);
    }

    public static Error Forbidden(string message, int errorId = 0)
    {
        return Error.Factory.CreateFor<T>(message, ErrorType.Forbidden, errorId);
    }

    public static Error Invalid(string message, int errorId = 0)
    {
        return Error.Factory.Invalid<T>(message, errorId);
    }

    public static Error UnAuthorized(int errorId = 0)
    {
        return Error.Factory.UnAuthorized<T>(errorId);
    }

    public static Error NotImplemented(int errorId = 0)
    {
        return Error.Factory.CreateFor<T>("The requested operation is not implemented.", ErrorType.Invalid, errorId);
    }
}
