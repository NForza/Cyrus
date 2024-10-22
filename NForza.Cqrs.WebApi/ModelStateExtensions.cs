using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NForza.Cqrs.WebApi;

public static class ModelStateExtensions
{
    public static ModelStateDictionary AddErrorsFromCommandResult(this ModelStateDictionary modelState, CommandResult commandResult)
    {
        commandResult.Errors.ToList().ForEach((m) => { modelState.AddModelError("", m.ErrorMessage); });
        return modelState;
    }
}