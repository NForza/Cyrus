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
                var command = commandObj == null ? new DeleteCustomerCommand() : (DeleteCustomerCommand)commandObj;
                string? guid = context.Request.Headers["CompanyId"].FirstOrDefault();
                if (guid != null)
                {
                    command.Id = new CustomerId(guid);
                    return AugmentationResult.Success(command);
                }
                else
                {
                    return AugmentationResult.Failed(Results.BadRequest("CompanyId header is required"));
                }
            })
            .Delete("{Id}")
            .AcceptedOnEvent<CustomerUpdatedEvent>("/customers/{Id}")
            .OtherwiseFail();

        QueryEndpoint<AllCustomersQuery>()
            .Get("");

        QueryEndpoint<CustomerByIdQuery>()
            .Get("{Id}");
    }
}
