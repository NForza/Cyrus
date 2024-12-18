using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using NForza.Cyrus.WebApi;
using NForza.Cyrus.WebApi.Policies;

namespace DemoApp.WebApi;

public class CustomerEndpointGroup : EndpointGroup
{
    public CustomerEndpointGroup() : base("Customers")
    {
        CommandEndpoint<AddCustomerCommand>()
            .Post("")
            .AcceptedOnEvent<CustomerAddedEvent>("/customers/{Id}")
            .OtherwiseFail();

        CommandEndpoint<UpdateCustomerCommand>()
            .Put("")
            .AcceptedOnEvent<CustomerUpdatedEvent>("/customers/{Id}")
            .OtherwiseFail();

        CommandEndpoint<DeleteCustomerCommand>()
            .AugmentInput((commandObj, context) =>
            {
                if (commandObj == null)
                    return AugmentationResult.Failed(Results.BadRequest());
                string? apiKey = context.Request.Headers["ApiKey"].FirstOrDefault();
                if (apiKey != null)
                {
                    return AugmentationResult.Success(commandObj);
                }
                return AugmentationResult.Failed(Results.BadRequest("ApiKey not present"));
            })
            .Delete("{Id}")
            .AcceptedWhenSucceeded()
            .OtherwiseFail();

        QueryEndpoint<AllCustomersQuery>()
            .Get("");

        QueryEndpoint<CustomerByIdQuery>()
            .Get("{Id}");
    }
}
