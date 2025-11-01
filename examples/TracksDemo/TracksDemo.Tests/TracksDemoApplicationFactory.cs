using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TracksDemo.Tests;

internal class TracksDemoApplicationFactory(ITestOutputHelper outputWindow) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            if (outputWindow != null)
            {
                logging.AddXUnit(outputWindow);
            }
        });
    }
}
