using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DemoApp.WebApi.Tests;

internal class DemoAppTestClient
{
    private readonly WebApplicationFactory<Program> factory;

    public DemoAppTestClient(ITestOutputHelper testOutput)
    {
        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureLogging((ILoggingBuilder logging) => logging.AddXUnit(testOutput)));
    }

    public HttpClient CreateClient() => factory.CreateClient();
}