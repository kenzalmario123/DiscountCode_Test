using DiscountApp.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiscountApp.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContext<DiscountAppDbContext>(options =>
            options.UseInMemoryDatabase("DiscountAppDb"),
            ServiceLifetime.Singleton, // Use Singleton for background services
            ServiceLifetime.Singleton);

        services.AddSingleton<IDiscountAppDbContext>(provider => provider.GetRequiredService<DiscountAppDbContext>());
        
        return services;
    }
}