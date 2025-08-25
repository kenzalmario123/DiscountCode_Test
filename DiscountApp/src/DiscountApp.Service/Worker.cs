using DiscountApp.Service.Interfaces;

namespace DiscountApp.Service;

public class WorkerSetUp<W> : IWorkerSetup where W : BackgroundService
{
    private readonly IServiceCollection _services;

    public WorkerSetUp(IServiceCollection services)
    {
        _services = services;

        StartWorker<W>();
    }

    public void StartWorker<T>() where T : BackgroundService
    {

        // Register the service as transient so that it can be created each time is requested
        _services.AddHostedService<T>();
    }
}
