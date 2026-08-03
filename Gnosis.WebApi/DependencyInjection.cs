using Gnosis.Business.Services;
using Gnosis.Domain.Interfaces;
using Gnosis.Infrastructure;
using Gnosis.Infrastructure.Identity;
using Gnosis.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
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

        // 3. Cuentas de usuario: AddIdentityCore (no la versión completa con cookies/UI, que no
        // aplica aquí porque el cliente es Blazor WASM autenticado con JWT) da UserManager,
        // hash de contraseñas y las tablas AspNetUsers/AspNetRoles sobre el mismo GnosisDbContext.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Regla más simple que el default de Identity (que exige un caracter no alfanumérico):
                // suficiente para una app de productividad personal, sin volverla tediosa de usar.
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<GnosisDbContext>()
            // AddIdentityCore no registra proveedores de tokens por defecto (a diferencia de
            // AddIdentity). Sin esto, GenerateEmailConfirmationTokenAsync/GeneratePasswordResetTokenAsync
            // truenan con "No IUserTwoFactorTokenProvider<TUser> named 'Default' is registered".
            .AddDefaultTokenProviders();

        // 4. Inyectar Servicios Ocultos
        services.AddScoped<ITareaService, TareaService>();
        services.AddScoped<ISesionEnfoqueService, SesionEnfoqueService>();
        services.AddScoped<IBloqueTiempoService, BloqueTiempoService>();
        services.AddScoped<IEstadisticasService, EstadisticasService>();
        services.AddScoped<ITokenService, TokenService>();
        // AddHttpClient (no AddScoped) porque BrevoEmailSender necesita un HttpClient inyectado;
        // esto registra tanto el HttpClient administrado como el propio IEmailSender.
        services.AddHttpClient<IEmailSender, BrevoEmailSender>();

        return services;
    }
}