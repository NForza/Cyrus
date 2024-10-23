using DemoApp.WebApi;
using NForza.Cqrs;
using NForza.Cqrs.WebApi;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointGroup<CustomerEndpointGroup>();
        builder.Services.AddJsonConverters();
        builder.Services.AddCqrs(o => o.AddEndpoints().ConfigureJsonConverters());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.MapCqrs();
        app.UseHttpsRedirection();
        app.Run();
    }
}