using System.Net;
using System.Net.Http.Json;
using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using FluentAssertions;
using Xunit.Abstractions;

namespace DemoApp.WebApi.Tests;

public class CustomerTests(ITestOutputHelper testOutput)
{
    private readonly HttpClient client = new DemoAppTestClient(testOutput).CreateClient();

    [Theory]
    [InlineData("/customers?page=1&pageSize=10")]
    public async Task Getting_Customers_Should_Succeed(string url)
    {
        var response = await client.GetAsync(url);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_Should_Succeed(string url)
    {
        var command = new AddCustomerCommand(new Name("Thomas"), new Address("The Netherlands"), CustomerType.Private);
        var response = await client.PostAsJsonAsync(url, command);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location?.ToString().Should().NotBeNullOrEmpty();
        response.Headers.Location!.ToString().Should().StartWith("/customers/");
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Posting_Add_Customer_Command_Without_A_Name_Should_Return_Bad_Request(string url)
    {
        var command = new { Name = (string?)null, Address = "The Netherlands" };
        var response = await client.PostAsJsonAsync(url, command);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Putting_Update_Customer_Command_Should_Succeed(string url)
    {
        var command = new UpdateCustomerCommand(new CustomerId(), new Name("Thomas"), new Address("The Netherlands"));
        var response = await client.PutAsJsonAsync(url, command);
        response.Should().NotBeNull();
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location?.ToString().Should().NotBeNullOrEmpty();
        response.Headers.Location!.ToString().Should().StartWith("/customers/");
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Deleting_Customer_Command_Without_ApiKey_Header_Should_Fail(string url)
    {
        var command = new DeleteCustomerCommand(new CustomerId());
        var response = await client.DeleteAsync(url + $"/{command.Id}");
        response.Should().NotBeNull();
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain("ApiKey not present");
    }

    [Theory]
    [InlineData("/customers")]
    public async Task Deleting_Customer_Command_With_ApiKey_Header_Should_Succeed(string url)
    {
        var command = new DeleteCustomerCommand(new CustomerId());
        client.DefaultRequestHeaders.Add("ApiKey", "1234");
        var response = await client.DeleteAsync(url + $"/{command.Id}");
        response.Should().NotBeNull();
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }
}
