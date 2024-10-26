namespace NForza.Cqrs.WebApi.Policies;

public abstract class InputMappingPolicy
{
    public abstract Task<object> MapInputAsync(Type typeToCreate);
}