using CoreERP.Application.Interfaces;
using CoreERP.Infrastructure.Configuration;
using CoreERP.Infrastructure.Email;
using CoreERP.Infrastructure.Identity;
using CoreERP.Infrastructure.Persistence;
using CoreERP.Infrastructure.Persistence.Repositories;
using CoreERP.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 2);
                }));

        // Identity
        services.AddIdentity<ApplicationIdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Data Protection (token encryption)
        services.AddDataProtection()
            .SetApplicationName("CoreERP")
            .PersistKeysToFileSystem(new DirectoryInfo(
                Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys")));
        services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();

        // Microsoft Graph Service
        services.AddHttpClient();
        services.AddScoped<IMicrosoftGraphService, MicrosoftGraphService>();

        // Email Service
        services.AddScoped<IEmailService, SmtpEmailService>();

        // SignalR
        services.AddSignalR();

        // Teams Module Configuration
        services.Configure<TeamsOptions>(configuration.GetSection(TeamsOptions.SectionName));
        services.AddSingleton<ITeamsConfigurationService, TeamsConfigurationService>();


        // Notification Services
        services.AddScoped<INotificaRepository, NotificaRepository>();
        services.AddScoped<IPreferenzaNotificaRepository, PreferenzaNotificaRepository>();
        services.AddScoped<ISottoscrizioneNotificaRepository, SottoscrizioneNotificaRepository>();
        services.AddScoped<ITeamsNotificationService, TeamsNotificationService>();
        services.AddScoped<INotificaService, NotificaService>();
        services.AddHostedService<NotificaCleanupService>();

        // Anagrafica Services
        services.AddScoped<IAnagraficaRepository, AnagraficaRepository>();
        services.AddScoped<IContattoRepository, ContattoRepository>();
        services.AddScoped<IRuoloContattoRepository, RuoloContattoRepository>();
        services.AddScoped<IMotivoDisattivazioneRepository, MotivoDisattivazioneRepository>();
        services.AddScoped<IMetodoPagamentoRepository, MetodoPagamentoRepository>();
        services.AddScoped<ITipoTecnologiaRepository, TipoTecnologiaRepository>();
        services.AddScoped<IReteRiferimentoRepository, ReteRiferimentoRepository>();
        services.AddScoped<IStoricoModificaRepository, StoricoModificaRepository>();
        services.AddScoped<IAnagraficaService, AnagraficaService>();

        // Messaggi
        services.AddScoped<IMessaggioRepository, MessaggioRepository>();

        // Indirizzo & EGON Services
        services.AddScoped<IIndirizzoRepository, IndirizzoRepository>();
        services.AddScoped<IEgonService, EgonService>();

        return services;
    }
}
