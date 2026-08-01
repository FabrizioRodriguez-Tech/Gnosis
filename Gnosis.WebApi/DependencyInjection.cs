using Gnosis.Business.Services;
using Gnosis.Domain.Interfaces;
using Gnosis.Infrastructure;
using Gnosis.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gnosis.WebApi;

internal static class DependencyInjection
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configurar la Base de Datos
        services.AddDbContext<GnosisDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // 2. Inyectar el Repositorio Genérico (El único que necesitas)
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        // 3. Inyectar Servicios Ocultos
        services.AddScoped<ITareaService, TareaService>();
        services.AddScoped<ISesionEnfoqueService, SesionEnfoqueService>();
        services.AddScoped<IBloqueTiempoService, BloqueTiempoService>();
        services.AddScoped<IEstadisticasService, EstadisticasService>();

        return services;
    }
}