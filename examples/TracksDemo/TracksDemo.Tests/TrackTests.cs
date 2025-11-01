using FluentAssertions;
using Xunit.Abstractions;

namespace TracksDemo.Tests;

public class TrackTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Test1()
    {
        TracksDemoApplicationFactory webApplicationFactory = new(outputWindow);
        var client = webApplicationFactory.CreateClient();

        var result = await client.GetAsync("/tracks");
        result.Should().NotBeNull();
        result.EnsureSuccessStatusCode();
    }
}
