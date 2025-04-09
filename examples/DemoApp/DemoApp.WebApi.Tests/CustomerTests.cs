using System.Net;
using System.Net.Http.Json;
using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;
using Xunit.Abstractions;

namespace DemoApp.WebApi.Tests;

public class CustomerTests
{
    private readonly HttpClient client;
    private readonly IServiceProvider services;
    private readonly RecordingLocalEventBus eventBus;

    public CustomerTests(ITestOutputHelper testOutput)
    {
       (client, services) = new DemoAppTestClient(testOutput)
            .CreateClientAndServiceProvider();
        eventBus = (RecordingLocalEventBus) services.GetRequiredService<IEventBus>();
    }

    [Theory]
    [InlineData("/customers/1/10")]
    public async Task Getting_Customers_Should_Succeed(string url)
    {
        var response = await client.GetAsync(url);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/customers/0c81023e-8dbb-4cab-95a9-99f6057f81de")]
    public async Task Getting_Customer_By_ID_Should_Succeed(string url)
    {
        var response = await client.GetAsync(url);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_Should_Succeed(string url)
    {
        var command = new AddCustomerCommand(new CustomerId(), new Name("Thomas"), new Address(new Street("Main Street"), new StreetNumber(12)), CustomerType.Private);
        var response = await client.PostAsJsonAsync(url, command);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location?.ToString().Should().NotBeNullOrEmpty();
        response.Headers.Location!.ToString().Should().StartWith("/customers/");
    }

    [Theory(Skip = "Waiting for bug fix")]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_Without_A_Name_Should_Return_Bad_Request(string url)
    {
        var command = new { Name = (string?)null, Address = "The Netherlands" };
        var response = await client.PostAsJsonAsync(url, command);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Name can't be empty");
    }

    [Theory(Skip ="Waiting for bug fix")]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_Without_Number_Should_Return_Bad_Request(string url)
    {
        var command = new { Name = (string?)null, Address = new { Street = "The Netherlands", StreetNumber = 0 } };
        var response = await client.PostAsJsonAsync(url, command);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Street number must be greater than 0");
    }

    [Theory]
    [InlineData("/customers/{Id}")]
    public async Task Putting_Update_Customer_Command_Should_Succeed(string url)
    {
        var customerId = new CustomerId();
        var command = new UpdateCustomerCommand(customerId, new Name("Thomas"), new Address(new Street("Main Street"), new StreetNumber(12)));
        var response = await client.PutAsJsonAsync(url.Replace("{Id}", customerId.ToString()), command);
        response.Should().NotBeNull();
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location?.ToString().Should().NotBeNullOrEmpty();
        response.Headers.Location!.ToString().Should().StartWith("/customers/");

        var customerAddedEvent = eventBus!.GetEvent<CustomerUpdatedEvent>();
        customerAddedEvent.Should().NotBeNull();
        customerAddedEvent!.Id.Should().Be(customerId);
    }
}
