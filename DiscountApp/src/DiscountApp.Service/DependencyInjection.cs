using DiscountApp.Service.Interfaces;
using DiscountApp.Service.Services;

namespace DiscountApp.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddService(this IServiceCollection services)
    {
        services.AddSingleton<IDiscountCodeService, DiscountCodeService>();

        return services;
    }
}