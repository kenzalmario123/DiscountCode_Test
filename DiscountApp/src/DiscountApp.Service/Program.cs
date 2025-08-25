using DiscountApp.Persistence;
using DiscountApp.Service;
using DiscountApp.Service.Interfaces;
using DiscountApp.Service.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure dependency injection for services and persistence
builder.Services.AddService();
builder.Services.AddPersistence();

builder.Services.AddHostedService<TcpBackgroundService>();

builder.Services.Configure<HostOptions>(opts =>
{
    opts.ShutdownTimeout = TimeSpan.FromSeconds(30);
});


builder.Services.AddSingleton<IWorkerSetup>(new WorkerSetUp<TcpBackgroundService>(builder.Services));

var host = builder.Build();
await host.RunAsync();
