namespace NForza.Cqrs.WebApi;

public interface ICustomRequestHandler
{
    Delegate CreateRequestHandler();
}
