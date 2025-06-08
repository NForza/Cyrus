using Alba;

namespace DemoApp.WebApi.Tests;

public static class ScenarioExtensions
{
    public static Scenario JsonBody<T>(this Scenario scenario, T obj) 
    {
        scenario.WriteJson(obj, null);
        return scenario;
    }
}
