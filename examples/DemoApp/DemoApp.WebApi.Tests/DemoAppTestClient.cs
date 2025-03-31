using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Cqrs;
using Xunit.Abstractions;

namespace DemoApp.WebApi.Tests;

internal class DemoAppTestClient
{
    private readonly WebApplicationFactory<Program> factory;

    public DemoAppTestClient(ITestOutputHelper testOutput)
    {
        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IEventBus, RecordingLocalEventBus>();
                });
                builder.ConfigureLogging((ILoggingBuilder logging) => logging.AddXUnit(testOutput));
            });
    }

    public HttpClient CreateClient() => factory.CreateClient();
    public (HttpClient, IServiceProvider) CreateClientAndServiceProvider()
    {
        return (factory.CreateClient(), factory.Services);
    }
}