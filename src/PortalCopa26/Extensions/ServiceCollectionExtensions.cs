using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Services;
using PortalCopa26.Services.Charts;

namespace PortalCopa26.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra o acesso a dados do PortalCopa26 via <see cref="IDbContextFactory{AppDbContext}"/>,
    /// adequado a Blazor Server (D3): cada operação cria um contexto de vida curta,
    /// em vez de compartilhar um DbContext por todo o circuito do usuário.
    /// </summary>
    public static IServiceCollection AddPortalCopaData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PortalCopa26")
            ?? throw new InvalidOperationException("Connection string 'PortalCopa26' não configurada.");

        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

        return services;
    }

    /// <summary>Registra os serviços de acesso a dados consumidos pela Landing Page.</summary>
    public static IServiceCollection AddLandingPageServices(this IServiceCollection services)
    {
        services.AddScoped<ILandingPageService, LandingPageService>();
        services.AddScoped<IChartInterop, ChartInterop>();

        return services;
    }
}
