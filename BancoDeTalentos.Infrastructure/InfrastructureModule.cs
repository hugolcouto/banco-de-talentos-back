using BancoDeTalentos.Infrastructure.Persistence;
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
            .AddData(configuration);

        return services;
    }

    private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BancoDeTalentosDbContext>();

        string dbConnectionString = configuration.GetConnectionString("SqlConnectionString")!;

        services.AddDbContext<BancoDeTalentosDbContext>(
            o => o.UseInMemoryDatabase("BancoDeTalentosMemoryDb")
        );

        // services.AddDbContext<BancoDeTalentosDbContext>(
        //     o => o.UseSqlServer(dbConnectionString)
        // );

        return services;
    }
}
