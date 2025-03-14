namespace DemoApp.WebApi
{
    //public class CommandCyrusStartup : ICyrusWebStartup
    //{
    //    public void AddStartup(IEndpointRouteBuilder app)
    //    {
    //        app.MapPost("/test", ([FromBody] UpdateCustomerCommand command, [FromServices] IEventBus eventBus) =>
    //        {
    //            if (!ObjectValidation.Validate<AddCustomerCommand>(command, app.ServiceProvider, out var problem))
    //                return Results.BadRequest(problem);
    //            return new CommandResultAdapter(eventBus).FromIResultAndEvents(UpdateCustomerCommandHandler.Execute(command));
    //        });

    //        app.MapPost("/test2", ([FromBody] AddCustomerCommand command, [FromServices] IEventBus eventBus) =>
    //        {
    //            if (!ObjectValidation.Validate<AddCustomerCommand>(command, app.ServiceProvider, out var problem))
    //                return Results.BadRequest(problem);
    //            var commandHandler = app.ServiceProvider.GetRequiredService<AddCustomerCommandHandler>();
    //            return new CommandResultAdapter(eventBus).FromEvents(commandHandler.Execute(command));
    //        });
    //    }
    //}
}
