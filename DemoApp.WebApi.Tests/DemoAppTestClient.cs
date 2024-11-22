
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace DemoApp.WebApi.Tests
{
    internal class DemoAppTestClient 
    {
        private readonly WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();

        public HttpClient CreateClient() => factory.CreateClient();
    }
}