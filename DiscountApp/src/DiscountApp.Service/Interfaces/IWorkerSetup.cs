namespace DiscountApp.Service.Interfaces;

public interface IWorkerSetup
{
    void StartWorker<T>() where T : BackgroundService;
}