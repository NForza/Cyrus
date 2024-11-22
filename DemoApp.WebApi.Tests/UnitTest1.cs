using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DemoApp.WebApi.Tests
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("/customers")]
        public async Task Getting_Customers_Should_Succeed(string url)
        {
            var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            var response = await client.GetAsync(url);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
