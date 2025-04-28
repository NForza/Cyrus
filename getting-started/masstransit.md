## Add MassTransit to an existing Cyrus Application

* Install the NForza.Cyrus.MassTransit package

* Configure the Cyrus generators by adding the following class to the application:

```csharp
using NForza.Cyrus.Abstractions;

namespace MyCyrusApp;

public class CyrusConfiguration: CyrusConfig
{
    public CyrusConfiguration()
    {
        UseMassTransit();
    }
}
```
* Configure MassTransit in the Program.cs in the normal way, for example:

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
    x.UsingInMemory((context, cfg) =>cfg.ConfigureEndpoints(context));
});
```
* That's all. 
