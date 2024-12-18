namespace NForza.Cyrus.Cqrs;

public class CommandError(string errorKey, string errorMessage)
{
    public string ErrorKey { get; set; } = errorKey;
    public string ErrorMessage { get; set; } = errorMessage;

    public static CommandError FromException(Exception ex) => new(ex.GetType().FullName!, ex.Message);
}
