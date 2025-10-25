using System.Threading.Tasks;
using Alba;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Cqrs;
using Xunit.Abstractions;

namespace DemoApp.WebApi.Tests;

internal static class DemoAppTestClient
{
    public static async Task<IAlbaHost> GetHostAsync(ITestOutputHelper testOutput)
    {
        return await AlbaHost.For<Program>(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IMessageBus, RecordingLocalMessageBus>();
            });
            builder.ConfigureLogging((ILoggingBuilder logging) => logging.AddXUnit(testOutput));
        });
    }
}