using Microsoft.AspNetCore.Mvc.Testing;

namespace DemoApp.WebApi.Tests
{
    internal class DemoAppTestClient 
    {
        private readonly WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();

        public HttpClient CreateClient() => factory.CreateClient();
    }
}