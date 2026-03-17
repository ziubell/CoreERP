using CoreERP.Application.Interfaces;
using CoreERP.Infrastructure.Configuration;
using CoreERP.Infrastructure.Email;
using CoreERP.Infrastructure.Identity;
using CoreERP.Infrastructure.Persistence;
using CoreERP.Infrastructure.Persistence.Repositories;
using CoreERP.Infrastructure.Services;
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

        return services;
    }
}
