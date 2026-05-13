using BancoDeTalentos.Core.Interfaces;
using BancoDeTalentos.Infrastructure.Persistence;
using BancoDeTalentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BancoDeTalentos.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRepositories()
            .AddData(configuration);

        return services;
    }

    private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        string dbConnectionString = configuration.GetConnectionString("SqlConnectionString")!;

        services.AddDbContext<BancoDeTalentosDbContext>(
            o => o.UseInMemoryDatabase("BancoDeTalentosMemoryDb")
        );

        // services.AddDbContext<BancoDeTalentosDbContext>(
        //     o => o.UseSqlServer(dbConnectionString)
        // );

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IJobRepository, JobRepository>();

        return services;
    }
}
