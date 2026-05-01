using Microsoft.Extensions.DependencyInjection;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Services;

namespace BancoDeTalentos.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
