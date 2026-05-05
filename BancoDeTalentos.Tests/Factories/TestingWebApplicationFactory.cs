using BancoDeTalentos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BancoDeTalentos.Tests.Factories;

public class TestingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(
            services =>
            {
                // Cria o nome do banco de dados UMA VEZ por execução da Factory
                string dbName = Guid.NewGuid().ToString();

                List<ServiceDescriptor>? descriptors = services.Where(
                    d => d.ServiceType.Name.Contains("DbContextOptions")
                ).ToList();

                foreach (ServiceDescriptor descriptor in descriptors)
                    services.Remove(descriptor);

                services.AddDbContext<BancoDeTalentosDbContext>(
                    options => options.UseInMemoryDatabase(dbName)
                );
            }
        );
    }
}
