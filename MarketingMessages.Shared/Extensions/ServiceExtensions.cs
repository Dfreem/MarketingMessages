using Microsoft.Extensions.DependencyInjection;
using MarketingMessages.Shared.Services;
using MarketingMessages.Shared.Components.Toast.Models;
using MarketingMessages.Shared.Services.ToastService;
namespace MarketingMessages.Shared.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddToast(config =>
        {
            config.Position = ToastPosition.Top | ToastPosition.Right;
            config.Duration = 3000;
        });
        return services;
    }
}
