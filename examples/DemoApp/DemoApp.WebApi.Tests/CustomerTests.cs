using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Alba;
using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;
using Xunit.Abstractions;

namespace DemoApp.WebApi.Tests;

public class CustomerTests( ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData("/customers/1/10")]
    public async Task Getting_Customers_Should_Succeed(string url)
    {
        var host = await DemoAppTestClient.GetHostAsync(outputHelper);
        var result = await host.Scenario(_ =>
        {
            _.Get.Url(url);
            _.StatusCodeShouldBeOk();
        });
    }

    [Theory]
    [InlineData("/customers/0c81023e-8dbb-4cab-95a9-99f6057f81de")]
    public async Task Getting_Customer_By_ID_Should_Succeed(string url)
    {
        var host = await DemoAppTestClient.GetHostAsync(outputHelper);
        var result = await host.Scenario(_ =>
        {
            _.Get.Url(url);
            _.StatusCodeShouldBeOk();
        });
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_Should_Succeed(string url)
    {
        var command = new AddCustomerCommand(new CustomerId(), new Name("Thomas"), new Address(new Street("Main Street"), new StreetNumber(12)), CustomerType.Private);
        var host = await DemoAppTestClient.GetHostAsync(outputHelper);
        var result = await host.Scenario(_ =>
        {
            _.JsonBody(command);
            _.Post.Url(url);
            _.StatusCodeShouldBe(HttpStatusCode.Accepted);
        });
        result.Context.Response.Headers.Location.ToString().Should().NotBeNullOrEmpty();
        result.Context.Response.Headers.Location.ToString().Should().StartWith("/customers/");
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_Without_A_Name_Should_Return_Bad_Request(string url)
    {
        var command = new AddCustomerCommand
        {
            Id = new CustomerId(),
            CustomerType = CustomerType.Private,
            Name = new Name(""),
            Address = new Address
            {
                Street = new Street("The Netherlands"),
                StreetNumber = new(1)
            }
        };
        var host = await DemoAppTestClient.GetHostAsync(outputHelper);
        var response = await host.Scenario(_ =>
          {
              _.JsonBody(command);
              _.Post.Url(url);
              _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
          });
        response.Should().NotBeNull();
        var content = await response.ReadAsTextAsync();
        content.Should().Contain("Name can't be empty");
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_With_Number_Zero_Should_Return_Bad_Request(string url)
    {
        var command = new AddCustomerCommand { Id = new CustomerId(), CustomerType = CustomerType.Private, Name = new Name("Test"), Address = new Address { Street = new Street("The Netherlands"), StreetNumber = new(0) } };
        var host = await DemoAppTestClient.GetHostAsync(outputHelper);
        var response = await host.Scenario(_ =>
        {
            _.JsonBody(command);
            _.Post.Url(url);
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
        response.Should().NotBeNull();
        var content = await response.ReadAsTextAsync();
        content.Should().Contain("Street number must be greater than 0");
    }

    [Theory]
    [InlineData("/customers/{Id}")]
    public async Task Putting_Update_Customer_Command_Should_Succeed(string url)
    {
        var customerId = new CustomerId();
        var command = new UpdateCustomerCommand(customerId, new Name("Thomas"), new Address(new Street("Main Street"), new StreetNumber(12)));

        var host = await DemoAppTestClient.GetHostAsync(outputHelper);

        var response = await host.Scenario( _ => 
        {
            _.JsonBody(command);
            _.Put.Url(url.Replace("{Id}", customerId.ToString()));
            _.StatusCodeShouldBe(HttpStatusCode.Accepted);
        });
        response.Should().NotBeNull();
        var content = await response.ReadAsTextAsync();

        response.Context.Response.Headers.Location.ToString().Should().NotBeNullOrEmpty();
        response.Context.Response.Headers.Location.ToString().Should().StartWith("/customers/");

        var eventBus = host.Services.GetRequiredService<IEventBus>() as RecordingLocalEventBus;
        var customerAddedEvent = eventBus!.GetMessage<CustomerUpdatedEvent>();
        customerAddedEvent.Should().NotBeNull();
        customerAddedEvent!.Id.Should().Be(customerId);
    }
}
