namespace NForza.Cyrus;

public enum ErrorType
{
    None = 0,
    NotFound = 404,
    Invalid = 400,
    Conflict = 409,
    Unauthorized = 401,
    Forbidden = 403,
    Timeout = 408,
    Canceled = 499,
    Failure = 500,
    BusinessRuleViolation = 422,
}