using InvestCaixa.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<InvestimentoDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<InvestimentoDbContext>(options =>
            {
                options.UseInMemoryDatabase("InvestCaixaTests");
            });
        });
    }
}