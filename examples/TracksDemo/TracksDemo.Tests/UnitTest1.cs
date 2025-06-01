using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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

public class UnitTest1(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Test1()
    {
        TracksDemoApplicationFactory webApplicationFactory = new(outputWindow);
        var client = webApplicationFactory.CreateClient();

        var result = await  client.GetAsync("/tracks");
        result.Should().NotBeNull();
        result.EnsureSuccessStatusCode();
    }
}
