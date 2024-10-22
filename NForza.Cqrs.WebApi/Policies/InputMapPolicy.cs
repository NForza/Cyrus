namespace NForza.Cqrs.WebApi;

public abstract class InputMappingPolicy
{
    public abstract Task<object> MapInputAsync(Type typeToCreate);
}