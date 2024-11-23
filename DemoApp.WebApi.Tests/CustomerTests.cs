using System.Net;
using System.Net.Http.Json;
using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using FluentAssertions;

namespace DemoApp.WebApi.Tests
{
    public class CustomerTests
    {
        private readonly HttpClient client = new DemoAppTestClient().CreateClient();

        [Theory]
        [InlineData("/customers")]
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
            var command = new AddCustomerCommand(new Name("Thomas"), new Address("The Netherlands"));
            var response = await client.PostAsJsonAsync(url, command);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            response.Headers.Location?.ToString().Should().NotBeNullOrEmpty();
            response.Headers.Location!.ToString().Should().StartWith("/customers/");
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
    }
}
